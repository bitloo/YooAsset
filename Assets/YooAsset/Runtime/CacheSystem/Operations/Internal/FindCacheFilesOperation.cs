﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class FindCacheFilesOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			FindPrepare,
			FindBundleFiles,
			FindRawFiles,
			Done,
		}

		private readonly string _packageName;
		private float _verifyStartTime;
		private IEnumerator<DirectoryInfo> _bundleFilesEnumerator = null;
		private IEnumerator<DirectoryInfo> _rawFilesEnumerator = null;
		private ESteps _steps = ESteps.None;

		/// <summary>
		/// 需要验证的元素
		/// </summary>
		public readonly List<VerifyElement> VerifyElements = new List<VerifyElement>(5000);

		public FindCacheFilesOperation(string packageName)
		{
			_packageName = packageName;
		}
		internal override void Start()
		{
			_steps = ESteps.FindPrepare;
			_verifyStartTime = UnityEngine.Time.realtimeSinceStartup;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.FindPrepare)
			{
				// BundleFiles
				{
					string rootPath = PersistentHelper.GetCachedBundleFileFolderPath(_packageName);
					DirectoryInfo rootDirectory = new DirectoryInfo(rootPath);
					if (rootDirectory.Exists)
					{
						var directorieInfos = rootDirectory.EnumerateDirectories();
						_bundleFilesEnumerator = directorieInfos.GetEnumerator();
					}
				}

				// RawFiles
				{
					string rootPath = PersistentHelper.GetCachedRawFileFolderPath(_packageName);
					DirectoryInfo rootDirectory = new DirectoryInfo(rootPath);
					if (rootDirectory.Exists)
					{
						var directorieInfos = rootDirectory.EnumerateDirectories();
						_rawFilesEnumerator = directorieInfos.GetEnumerator();
					}
				}

				_steps = ESteps.FindBundleFiles;
			}

			if (_steps == ESteps.FindBundleFiles)
			{
				if (UpdateFindBundleFiles())
					return;

				_steps = ESteps.FindRawFiles;
			}

			if (_steps == ESteps.FindRawFiles)
			{
				if (UpdateFindRawFiles())
					return;

				// 注意：总是返回成功
				_steps = ESteps.Done;
				Status = EOperationStatus.Succeed;
				float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyStartTime;
				YooLogger.Log($"Find cache files elapsed time {costTime:f1} seconds");
			}
		}

		private bool UpdateFindBundleFiles()
		{
			if (_bundleFilesEnumerator == null)
				return false;

			bool isFindItem;
			while (true)
			{
				isFindItem = _bundleFilesEnumerator.MoveNext();
				if (isFindItem == false)
					break;

				var fileFoder = _bundleFilesEnumerator.Current;
				string cacheGUID = fileFoder.Name;
				if (CacheSystem.IsCached(_packageName, cacheGUID))
					continue;

				// 创建验证元素类
				string fileRootPath = fileFoder.FullName;
				string dataFilePath = $"{fileRootPath}/{ YooAssetSettings.CacheBundleDataFileName}";
				string infoFilePath = $"{fileRootPath}/{ YooAssetSettings.CacheBundleInfoFileName}";
				VerifyElement element = new VerifyElement(_packageName, cacheGUID, fileRootPath, dataFilePath, infoFilePath);
				VerifyElements.Add(element);

				if (OperationSystem.IsBusy)
					break;
			}

			return isFindItem;
		}
		private bool UpdateFindRawFiles()
		{
			if (_rawFilesEnumerator == null)
				return false;

			bool isFindItem;
			while (true)
			{
				isFindItem = _rawFilesEnumerator.MoveNext();
				if (isFindItem == false)
					break;

				var fileFoder = _rawFilesEnumerator.Current;
				string cacheGUID = fileFoder.Name;
				if (CacheSystem.IsCached(_packageName, cacheGUID))
					continue;

				// 获取数据文件的后缀名
				string dataFileExtension = string.Empty;
				var fileInfos = fileFoder.GetFiles();
				foreach (var fileInfo in fileInfos)
				{
					if (fileInfo.Extension == ".temp")
						continue;

					if (fileInfo.Name.StartsWith(YooAssetSettings.CacheBundleDataFileName))
					{
						dataFileExtension = fileInfo.Extension;
						break;
					}
				}

				// 创建验证元素类
				string fileRootPath = fileFoder.FullName;
				string dataFilePath = $"{fileRootPath}/{ YooAssetSettings.CacheBundleDataFileName}{dataFileExtension}";
				string infoFilePath = $"{fileRootPath}/{ YooAssetSettings.CacheBundleInfoFileName}";
				VerifyElement element = new VerifyElement(_packageName, cacheGUID, fileRootPath, dataFilePath, infoFilePath);
				VerifyElements.Add(element);

				if (OperationSystem.IsBusy)
					break;
			}

			return isFindItem;
		}
	}
}
// Copyright (c) Improbable Worlds Ltd, All Rights Reserved
// ===========
// DO NOT EDIT - this file is automatically regenerated.
// =========== 
using System;
using System.Collections;
using System.Collections.Generic;
using Improbable.Unity.Configuration;
using UnityEngine;

namespace Improbable.Unity.Core
{
    internal static class CloudAssemblyArtifactResolver
    {
        public static IEnumerator ResolveAssetUrls(string deploymentName, string assemblyName, WorkerConfiguration workerConfiguration, Action<Dictionary<string, string>> onAssetsResolved, Action<Exception> onFailed)
        {
            var headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" }
            };

            var url = string.Format("{0}/assembly_content/v3/{1}/{2}/artifacts", workerConfiguration.InfraServiceUrl, workerConfiguration.AppName, assemblyName);
            using (var www = new WWW(url, null, headers))
            {
                yield return www;

                if (www.error != null)
                {
                    onFailed(new Exception("Failed to retrieve assembly list: " + www.error));
                }

                var assetUrls = new Dictionary<string, string>();

	            var response = JsonUtility.FromJson<AssemblyResponse>(www.text);
                if (response.Artifacts == null)
                {
                    onFailed(new Exception(string.Format("No artifacts found at {0}", url)));
                }
                else
                {
                    for (var i = 0; i < response.Artifacts.Count; i++)
                    {
                        var artifact = response.Artifacts[i];
                        assetUrls[artifact.ArtifactId.Name] = artifact.Url;
                    }
                }

                onAssetsResolved(assetUrls);
            }
        }

        [Serializable]
        private class AssemblyResponse
        {
            [SerializeField]
#pragma warning disable 0649
            private List<AssemblyArtifact> artifacts;
#pragma warning restore 0649
            public List<AssemblyArtifact> Artifacts
            {
                get { return artifacts; }
            }
        }

        [Serializable]
        private class ArtifactId
        {
            [SerializeField]
#pragma warning disable 0649
            private string name;
#pragma warning restore 0649
            public string Name
            {
                get { return name; }
            }
        }

        [Serializable]
        private class AssemblyArtifact
        {
            [SerializeField]
#pragma warning disable 0649
            // ReSharper disable once InconsistentNaming
            private ArtifactId artifact_id;
#pragma warning restore 0649
            public ArtifactId ArtifactId
            {
                get { return artifact_id; }
            }

            [SerializeField]
#pragma warning disable 0649
            private string url;
#pragma warning restore 0649
            public string Url
            {
                get { return url; }
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

namespace SimpleLeaderboard
{
    /// <summary>
    /// Struct representing a leaderboard submission containing the basic information such as:
    /// <see cref="Name"/>, <see cref="Score"/> and <see cref="Timestamp"/>.
    /// </summary>
    [Serializable]
    public struct Submission
    {
        /// <summary>
        /// Database id of the submission.
        /// This can be used for updating an existing entry.
        /// </summary>
        /// <remarks>
        /// The reason that <see cref="_id"/> is named differently is to match MongoDB's id property and make JSON
        /// serialization easier.
        /// </remarks>
        public string _id;

        /// <summary>
        /// The name of the player.
        /// </summary>
        public string Name;

        /// <summary>
        /// The amount of points the player acquired.
        /// </summary>
        public float Score;

        /// <summary>
        /// When the score was acquired.
        /// </summary>
        public DateTime? Timestamp;

        /// <summary>
        /// Prints the values of all properties as a <see cref="string"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> containing the values of all properties.</returns>
        public override string ToString()
        {
            return string.Format("(Id: {0} Name: {1}, Score: {2}, Timestamp: {3}) ", _id, Name, Score, Timestamp);
        }

        /// <summary>
        /// Converts the <see cref="Submission"/> to a <see cref="Dictionary{String,String}"/>
        /// </summary>
        /// <returns>A <see cref="Dictionary{String,String}"/> containing all properties.</returns>
        public Dictionary<string, string> ToDictionary()
        {
            var dictionary = new Dictionary<string, string>()
            {
                {"_id", _id ?? string.Empty},
                {"name", Name},
                {"score", Score.ToString(CultureInfo.InvariantCulture)},
                {"timestamp", Timestamp.ToString()}
            };

            return dictionary;
        }
    }

    /// <summary>
    /// Handles web requests to the leaderboard backend.
    /// Though designed with <see cref="Submission"/> in mind as the datatype to be transferred, it can be used with
    /// others as well.
    /// </summary>
    public static class Leaderboard
    {
        /// <summary>
        /// The base url of the server that requests will be sent to.
        /// </summary>
        /// <example>
        /// https://mysimpleleaderboard.com
        /// </example>
        public static string BaseUrl { get; set; }

        /// <summary>
        /// The default path that will be appended to <see cref="BaseUrl"/>.
        /// </summary>
        /// <example>
        /// /scores
        /// </example>
        public static string DefaultPath { get; set; }

        /// <summary>
        /// Sends a POST request to <see cref="BaseUrl"/>/<paramref name="path"/> with <paramref name="formData"/>
        /// in the request body.
        /// </summary>
        /// <param name="formData">A <see cref="Dictionary{String,String}"/> of data to be sent in the request.</param>
        /// <param name="callback">An optional callback that will receive the response as a JSON string.</param>
        /// <param name="path">The path to be appended to <see cref="BaseUrl"/> if provided, otherwise
        /// <see cref="DefaultPath"/> will be used.</param>
        public static IEnumerator Post(Dictionary<string, string> formData, Action<string> callback = null,
            string path = null)
        {
            var url = GetCompleteUrl(path);
            if (url == null)
            {
                yield break;
            }

            using (var webRequest = UnityWebRequest.Post(url, formData))
            {
                yield return webRequest.Send();
                string result = null;

                if (webRequest.isError)
                {
                    Debug.LogError(webRequest.error);
                } else if (webRequest.responseCode >= 400)
                {
                    Debug.LogErrorFormat("Error code: {0} - {1}", webRequest.responseCode, webRequest.downloadHandler.text);
                }
                else
                {
                    result = webRequest.downloadHandler.text;
                }

                if (callback != null)
                {
                    callback(result);
                }
            }
        }

        /// <summary>
        /// Sends a GET request to <see cref="BaseUrl"/>/<param name="path"> and retrieves all
        /// entries submitted.</param>
        /// </summary>
        /// <param name="callback">An optional callback that will receive the response as a JSON string.</param>
        /// <param name="path">The path to be appended to <see cref="BaseUrl"/> if provided, otherwise
        /// <see cref="DefaultPath"/> will be used.</param>
        /// <returns>A JSON <see cref="String"/> representing all entries submitted.</returns>
        public static IEnumerator Get(Action<string> callback = null, string path = null)
        {
            var url = GetCompleteUrl(path);
            if (url == null)
            {
                yield break;
            }

            using (var webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.Send();
                string result = null;

                if (webRequest.isError)
                {
                    Debug.LogError(webRequest.error);
                } else if (webRequest.responseCode >= 400)
                {
                    Debug.LogErrorFormat("Error code: {0} - {1}", webRequest.responseCode, webRequest.downloadHandler.text);
                }
                else
                {
                    result = webRequest.downloadHandler.text;
                }

                if (callback != null)
                {
                    callback(result);
                }
            }
        }

        /// <summary>
        /// Creates a url <see cref="string"/> by combining <see cref="BaseUrl"/> with <paramref name="path"/>.
        /// Resorts to using <see cref="DefaultPath"/> if <paramref name="path"/> is <c>null</c> or
        /// <see cref="string.Empty"/>.
        /// </summary>
        /// <param name="path">The path to be appended to <see cref="BaseUrl"/> if provided, otherwise
        /// <see cref="DefaultPath"/> will be used.</param>
        /// <returns>A url <see cref="string"/> consisting of <see cref="BaseUrl"/> and <paramref name="path"/> or
        /// <see cref="DefaultPath"/>.</returns>
        /// <remarks>Inserts a slash (/) between <see cref="BaseUrl"/> and <paramref name="path"/> if
        /// <see cref="BaseUrl"/> does not end with one and <paramref name="path"/> does not begin with one.</remarks>
        private static string GetCompleteUrl(string path)
        {
            if (string.IsNullOrEmpty(BaseUrl))
            {
                Debug.LogError("BaseUrl must be set.");
                return null;
            }

            if (string.IsNullOrEmpty(path))
            {
                path = DefaultPath ?? string.Empty;
            }
            var shouldInsertSlash = !path.StartsWith("/", StringComparison.OrdinalIgnoreCase) ||
                                    !BaseUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase);

            var url = string.Format("{0}{1}{2}", BaseUrl, shouldInsertSlash ? "/" : string.Empty, path);
            return url;
        }
    }
}

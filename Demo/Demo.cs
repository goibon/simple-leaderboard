using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace SimpleLeaderboard.Demo
{
    /// <summary>
    /// Demonstrates how <see cref="Leaderboard"/> can be used to store and retrieve
    /// leaderboard submissions.
    /// </summary>
    public class Demo : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("The base url of the requests to be sent. Eg. 'https://mySimpleLeaderboard.io'.")]
        public string BaseUrl;

        [Tooltip("The path to append to the base url. Eg. '/scores'.")]
        public string Path;

        [Header("UI References")]
        [Tooltip("InputField for entering a name.")]
        public InputField Name;
        [Tooltip("InputField for entering a score.")]
        public InputField Score;
        [Tooltip("GameObject that will contain the instances of ScorePrefab.")]
        public GameObject ContentView;
        [Tooltip("Root GameObject for the leaderboard.")]
        public GameObject LeaderboardPanel;

        [Header("Prefabs")]
        [Tooltip("Prefab of the object that will be used to populate the leaderboard." +
                 "This should contain two children with a Text component to show name and score.")]
        public GameObject ScorePrefab;

        private void Start()
        {
            // BaseUrl must be set
            Assert.IsFalse(string.IsNullOrEmpty(BaseUrl), "BaseUrl must be set.");

            // UI references must be set
            Assert.IsNotNull(Name, "InputField for Name must be set.");
            Assert.IsNotNull(Score, "InputField for Score must be set.");
            Assert.IsNotNull(ContentView, "ContentView must be set.");
            Assert.IsNotNull(LeaderboardPanel, "LeaderboardPanel must be set.");

            // Prefab must be set
            Assert.IsNotNull(ScorePrefab, "ScorePrefab must be set.");

            // Setup Leaderboard properties
            Leaderboard.BaseUrl = BaseUrl;
            Leaderboard.DefaultPath = Path;

            // Input validation
            Name.characterValidation = InputField.CharacterValidation.Name;
            Score.characterValidation = InputField.CharacterValidation.Decimal;
        }

        /// <summary>
        /// Sends the input entered into <see cref="Name"/> and <see cref="Score"/> to the backend using the
        /// <see cref="Leaderboard.Post"/> method and logs the result using <see cref="PostCallBack"/>.
        /// </summary>
        public void Post()
        {
            float score;
            if (!float.TryParse(Score.text, out score))
            {
                return;
            }
            var submission = new Submission();
            submission.Name = Name.text;
            submission.Score = score;
            StartCoroutine(Leaderboard.Post(submission.ToDictionary(), PostCallBack));
        }

        /// <summary>
        /// Example of a callback method defined outside of the call to <see cref="Leaderboard.Post"/>.
        /// This method logs the result.
        /// </summary>
        /// <param name="id">The database id of the entry created if the call to <see cref="Leaderboard.Post"/>
        /// succeeded.</param>
        private static void PostCallBack(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("Did not store submission correctly.");
            }
            else
            {
                Debug.LogFormat("Succesfully stored submission. Database id: {0}", id);
            }
        }

        /// <summary>
        /// Retrieves all entries stored in the backend using the <see cref="Leaderboard.Get"/> method and populates
        /// the UI leaderboard with the result.
        /// </summary>
        public void Get()
        {
            StartCoroutine(
                Leaderboard.Get(result =>
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        Debug.Log("Did not get any submissions.");
                        return;
                    }
                    var submissions = JsonConvert.DeserializeObject<Submission[]>(result);
                    PopulateLeaderboardUI(submissions);
                }));
        }

        /// <summary>
        /// Populates the UI leaderboard with the results of <see cref="Leaderboard.Get"/> if any.
        /// </summary>
        /// <param name="submissions">A <see cref="IList{Submission}"/> of entries retrieved from the backend.</param>
        private void PopulateLeaderboardUI(IList<Submission> submissions)
        {
            if (submissions.Count <= 0)
            {
                return;
            }

            for (var i = 0; i < submissions.Count; i++)
            {
                var scoreObject = Instantiate(ScorePrefab);
                scoreObject.transform.SetParent(ContentView.transform);
                scoreObject.transform.localPosition = Vector3.zero;
                var scoreRectTransform = scoreObject.GetComponent<RectTransform>();
                scoreRectTransform.localScale = Vector3.one;
                var childrenTextComponents = scoreObject.GetComponentsInChildren<Text>();
                childrenTextComponents[0].text = submissions[i].Name;
                childrenTextComponents[1].text = submissions[i].Score.ToString();
            }
            // Make sure the height of the content view fits the content
            var contentViewRectTransform = ContentView.GetComponent<RectTransform>();
            var contentViewLayoutGroup = ContentView.GetComponent<VerticalLayoutGroup>();
            contentViewRectTransform.sizeDelta = new Vector2(contentViewRectTransform.sizeDelta.x,
                Math.Abs(contentViewLayoutGroup.spacing * submissions.Count));

            LeaderboardPanel.SetActive(true);
        }
    }
}

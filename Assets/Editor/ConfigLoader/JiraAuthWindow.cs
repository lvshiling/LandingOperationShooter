using UnityEditor;
using UnityEngine;
using Assets.Editor.GBNEditor;

namespace Assets.Editor.ConfigLoader
{
    public class JiraAuthWindow : EditorWindow
    {

        private static string issueId = "";
        private static string jiraUser = "";
        private static string jiraPass = "";
        private static string gitPass = "";

        [MenuItem("Config/Авторизоваться в Jira|Git", false, 1)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<JiraAuthWindow>();
        }
        void OnEnable()
        {
            titleContent.text = "Jira Auth";
            if (issueId == "")
            {
                issueId = ConfigLoader.issueId;
            }
            if (jiraUser == "")
            {
                jiraUser = GBNEditorAccounts.Jira.account.name;
            }
            if (jiraPass == "")
            {
                jiraPass = GBNEditorAccounts.Jira.account.privateToken;
            }
            if (gitPass == "")
            {
                gitPass = GBNEditorAccounts.Git.account.privateToken;
            }
        }

        void OnGUI()
        {
            GUILayout.Label("Ссылка на Epic (Example: G3D-5953):", EditorStyles.boldLabel);
            issueId = EditorGUILayout.TextField("Epic ID", issueId);
            if (GUILayout.Button("Сохранить Epic ID"))
            {
                ConfigLoader.SaveIssueId(issueId);
                issueId = ConfigLoader.issueId;
            }
            if (!GBNEditorAccounts.Jira.account.IsEmpty() && !string.IsNullOrEmpty(ConfigLoader.issueId))
            {
                if (GUILayout.Button("Загрузить данные из Jira"))
                {
                    issueId = ConfigLoader.issueId;
                    ConfigLoader.SaveJiraAuth();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(ConfigLoader.issueId))
                {
                    GUILayout.Label("Для загрузки данных из Jira необходимо задать Epic ID", EditorStyles.label);
                }
                else if (GBNEditorAccounts.Jira.account.IsEmpty())
                {
                    GUILayout.Label("Для загрузки данных из Jira необходимо сохранить аккаунт Jira", EditorStyles.label);
                }
            }

            GUILayout.Label("Учетные данные Jira", EditorStyles.boldLabel);
            jiraUser = EditorGUILayout.TextField("Login", jiraUser);
            jiraPass = EditorGUILayout.PasswordField("Password", jiraPass);

            if (GUILayout.Button("Сохранить аккаунт Jira"))
            {
                string[] name = jiraUser.Split('@');
                if (GBNEditorAccounts.Jira.Login(name[0], jiraPass))
                    Debug.Log("<color=green><b>Jira Login and Password Saved!</b></color>");
                else
                    Debug.Log("<color=red><b>Jira Login and Password Incorrect!</b></color>");
                jiraUser = GBNEditorAccounts.Jira.account.name;
                jiraPass = GBNEditorAccounts.Jira.account.privateToken;
            }

            GUILayout.Label("Учетные данные Git", EditorStyles.boldLabel);
            gitPass = EditorGUILayout.PasswordField("Token", gitPass);

            if (GUILayout.Button("Сохранить токен Git"))
            {
                if (GBNEditorAccounts.Git.Login(gitPass))
                    Debug.Log("<color=green><b>Git Token Saved!</b></color>");
                else
                    Debug.Log("<color=red><b>Git Token Incorrect!</b></color>");
                gitPass = GBNEditorAccounts.Git.account.privateToken;
            }
        }
    }
}

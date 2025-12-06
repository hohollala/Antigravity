```
string launchJsonContent;
#if UNITY_EDITOR_OSX
                launchJsonContent = @"{
    ""version"": ""0.2.0"",
    ""configurations"": [
        {
            ""name"": ""Unity Editor"",
            ""type"": ""unity"",
            ""request"": ""attach""
        }
    ]
}";
#else
                // 플랫폼별 Unity Editor 경로 설정
                string unityEditorPath;
#if UNITY_EDITOR_WIN
                unityEditorPath = "${workspaceFolder}/Library/UnityEditor.exe";
#else
                unityEditorPath = "${workspaceFolder}/Library/UnityEditor";
#endif

                // launch.json 콘텐츠
                launchJsonContent = @$"{{
    ""version"": ""0.2.0"",
    ""configurations"": [
        {{
            ""name"": ""Unity Editor"",
            ""type"": ""unity"",
            ""request"": ""launch"",
            ""program"": ""{unityEditorPath}""
        }}
    ]
}}";
#endif
```
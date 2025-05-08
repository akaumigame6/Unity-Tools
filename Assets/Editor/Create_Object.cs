using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

public class Create_Objrct : EditorWindow
{
    public DefaultAsset targetFolder;
    private string targetFilePath;
    public MonoScript UI_Canvas_script; // 添付したいスクリプト
    public MonoScript ObjectMarker_script; // 添付したいスクリプト
    public List<MonoScript> Scripts = new List<MonoScript>(); // スクリプトのリスト
    public bool isCanvas = false; // Canvasを作成するかどうかのフラグ
    public bool isRoot = false; //  親オブジェクトにするかどうかのフラグ
    public bool isDestroyChild = false; // 子オブジェクトを削除するかどうかのフラグ
    public string RootObjectName = "RootObject"; // 親オブジェクトの名前
    private Vector2 scroll;
    public Vector3 position;//初期位置
    public Vector3 width;// 生成する位置

    List<GameObject> Cubes = new List<GameObject>();
    List<GameObject> Objects = new List<GameObject>();

    [MenuItem("Tools/オブジェクト配置")]
    public static void ShowWindow()
    {
        GetWindow<Create_Objrct>("Create Object");
    }

    private void OnGUI()
    {
        var currentTargetFolder = (DefaultAsset)EditorGUILayout.ObjectField("対象フォルダ", targetFolder, typeof(DefaultAsset), false);
        GUILayout.Space(10);

        if (currentTargetFolder != null)
        {
            targetFolder = currentTargetFolder;
            targetFilePath = AssetDatabase.GetAssetOrScenePath(targetFolder);
        }

        position = EditorGUILayout.Vector3Field("初期位置:", position);
        GUILayout.Space(10);

        width = EditorGUILayout.Vector3Field("生成位置:", width);
        GUILayout.Space(10);

        EditorGUILayout.LabelField("アタッチするScript", EditorStyles.boldLabel);
        GUILayout.Space(10);
        scroll = EditorGUILayout.BeginScrollView(scroll);

        // スクリプトのリストを表示・編集するためのGUIを作成
        for (int i = 0; i < Scripts.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // 要素の表示・編集
            Scripts[i] = (MonoScript)EditorGUILayout.ObjectField($"Element {i}", Scripts[i], typeof(MonoScript), false);

            // 削除ボタン
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                Scripts.RemoveAt(i);
                break; // リストが変更されたので抜ける
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // 追加ボタン
        if (GUILayout.Button("Add Element"))
        {
            Scripts.Add(null);
        }
        GUILayout.Space(10);
        isCanvas = EditorGUILayout.Toggle("Canvasを作成する", isCanvas);
        isRoot = EditorGUILayout.Toggle("ルートオブジェクトにする", isRoot);

        if (!isRoot){
            RootObjectName = EditorGUILayout.TextField("ルートオブジェクト名", RootObjectName);
        }
        GUILayout.Space(10);


        if (GUILayout.Button("Object Create")){
            CreateObjects(currentTargetFolder);
        }
        if (GUILayout.Button("Object_AllClear")){
            ClearObjects();
        }
    }

    void CreateObjects(DefaultAsset currentTargetFolder)
    {
        Debug.Log("CreateObject");
        if(currentTargetFolder==null)Debug.Log("NULL");
        else {
            Debug.Log(currentTargetFolder);
            targetFilePath=AssetDatabase.GetAssetPath(currentTargetFolder);
            var assetGuids = AssetDatabase.FindAssets("t:Model t:Prefab", new[] { targetFilePath });
            foreach (var assetGuid in assetGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGuid);
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Debug.Log($"AssetPath: {path}");
                Debug.Log($"obj:{obj}");

                GameObject instance = GameObject.Instantiate(obj,position,Quaternion.identity);
                // if(position.x<=25)position.x+=5;
                // else{position.x=0;position.z+=5;}
                position+= width;

                if(instance != null)
                {
                    // スクリプトをアタッチ
                    foreach (var script in Scripts)
                    {
                        if (script != null)
                        {
                            // スクリプトが同じオブジェクトのコンポーネントとしてアタッチされているかを確認
                            System.Type scriptType = script.GetClass();
                            if (scriptType != null && typeof(MonoBehaviour).IsAssignableFrom(scriptType))
                            {
                                if (instance.GetComponent(scriptType) == null)
                                {
                                    instance.AddComponent(scriptType);
                                }
                            }
                        }
                    }

                    if(isDestroyChild && instance.transform.childCount > 0){
                        foreach (Transform child in instance.transform){DestroyImmediate(child.gameObject);}
                    }
                    instance.name=obj.name;

                    // Canvasを作成するかどうかのフラグを確認
                    if (isCanvas)
                    {
                        CreateCanvas(instance);
                    }
                    // ルートオブジェクトにするかどうかのフラグを確認
                    if (!isRoot)
                    {
                        RootObject(instance);
                    }
                    else
                    {
                        instance.transform.SetParent(null);
                    }
                    Objects.Add(instance);
                }
                else Debug.Log("NULLですよ；；");
            }
        }
        Debug.Log("test");
    }

    void CreateCanvas(GameObject obj)
    {
        GameObject canvasObject = new GameObject(obj.name+"_c");
        canvasObject.transform.SetParent(obj.transform);
        Canvas Cube_Canvas = canvasObject.AddComponent<Canvas>();
        Cube_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
    }

    void RootObject(GameObject obj)
    {
        GameObject root = GameObject.Find (RootObjectName);
        if(root == null){
            root = new GameObject(RootObjectName);
        }
        root.transform.position = position;
        obj.transform.SetParent(root.transform);
    }

    void ClearObjects()
    {
        foreach (var obj in Objects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        Objects.Clear();
        position = Vector3.zero; // 初期位置に戻す
    }
}

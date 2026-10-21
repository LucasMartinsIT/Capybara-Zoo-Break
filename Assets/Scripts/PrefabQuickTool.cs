using UnityEngine;
using UnityEditor;

public class PrefabQuickTool
{
    [MenuItem("Tools/Quick Wrap into Prefab %#d")]
    public static void WrapAndPrefab()
    {
        foreach (GameObject selectedObj in Selection.gameObjects)
        {
            // 1. Cria o objeto pai na mesma posição exata do objeto selecionado
            GameObject parentWrapper = new GameObject(selectedObj.name);
            parentWrapper.transform.position = selectedObj.transform.position;
            parentWrapper.transform.rotation = Quaternion.identity;
            parentWrapper.transform.localScale = Vector3.one;

            // 2. Transfere a hierarquia mantendo a pose no mundo
            Transform oldParent = selectedObj.transform.parent;
            parentWrapper.transform.SetParent(oldParent, false);

            selectedObj.transform.SetParent(parentWrapper.transform, true);

            // 3. Salva automaticamente como Prefab na pasta Prefabs
            string localPath = "Assets/Prefabs/" + parentWrapper.name + ".prefab";

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
            PrefabUtility.SaveAsPrefabAssetAndConnect(parentWrapper, localPath, InteractionMode.UserAction);

            Debug.Log("<color=green>Prefab criado com sucesso em:</color> " + localPath);
        }
    }
}
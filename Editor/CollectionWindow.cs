using System;
using GitHubProject.collection_custom_window.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


// ReSharper disable once CheckNamespace
namespace Kabourlix.CollectionWindow.Editor
{
    public abstract class CollectionWindow<T> : EditorWindow where T : ScriptableObject, ICollectableForWindowEditor
    {
        protected string _resourcesRelativePath = "";
        protected string _resourcesPath = "";
        
        protected void EnableWindow()
        {
            if(string.IsNullOrEmpty(_resourcesRelativePath))
                throw new Exception("You must set the resources relative path before calling EnableWindow (ex : MyScriptableObject)");
            if(string.IsNullOrEmpty(_resourcesPath))
                throw new Exception("You must set the resources path before calling EnableWindow. (ex : Assets/Resources/)");
            //Import UXML
            var original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>
                ("Packages/com.kabourlix.collection-custom-window/Editor/Assets/CollectionWindow.uxml");
            //var original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>
                //("Assets/GitHubProject/collection-custom-window/Editor/Assets/CollectionWindow.uxml");
            var container = original.CloneTree();
            rootVisualElement.Add(container);

            //Import USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>
                ("Packages/com.kabourlix.collection-custom-window/Editor/Assets/CollectionWindowSheet.uss");
            //var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>
            //    ("Assets/GitHubProject/collection-custom-window/Editor/Assets/CollectionWindowSheet.uss");
            rootVisualElement.styleSheets.Add(styleSheet);
            
            CreateCardView();
            
            //Handle creation button
            var createButton = rootVisualElement.Q<Button>("create-button");
            createButton.clickable.clicked += CreateTFromButton;
        }

        /// <summary>
        /// Callback for the creation button of a T object in the editor window.
        /// </summary>
        private void CreateTFromButton()
        {
            var t = CreateInstance<T>();
            var path = EditorUtility.SaveFilePanelInProject("Save T", "new asset", "asset", "Save T"
                , _resourcesPath+ _resourcesRelativePath + "/");
            if(string.IsNullOrEmpty(path))
                throw new Exception("Path is null or empty");
            
            if(path.Split('.')[^1] != "asset")
                throw new Exception("File must be .asset");

            var aName = path.Split('/')[^1].Split('.')[0];
            t.Title = aName;
            
            AssetDatabase.CreateAsset(t, path);
            AssetDatabase.SaveAssets();

            CreateCardView();
        }

        
        /// <summary>
        /// Handle the creation of the card view for the collection window.
        /// </summary>
        private void CreateCardView()
        {
            if(string.IsNullOrEmpty(_resourcesRelativePath))
                throw new Exception("You must set the resources relative path before calling CreateCardView");
            
            FindAllTFromResources(_resourcesRelativePath, out var allT);

            var tList = rootVisualElement.Q<ListView>();
            
            tList.makeItem = () => new Label();
            tList.bindItem = (element, i) => ((Label) element).text = allT[i].Title;

            tList.itemsSource = allT;
            tList.fixedItemHeight = 16;
            tList.selectionType = SelectionType.Single;

            tList.onSelectionChange += (enumerable) =>
            {
                foreach (var it in enumerable)
                {
                    var tBox = rootVisualElement.Q("properties");
                    tBox.Clear();
                    
                    var t = it as T;
                    
                    var serializedT = new SerializedObject(t);
                    var tProperty = serializedT.GetIterator();
                    tProperty.Next(true);

                    while (tProperty.NextVisible(false))
                    {
                        var prop = new PropertyField(tProperty);
                        prop.SetEnabled(tProperty.name != "m_Script"); //Disable script field
                        prop.Bind(serializedT);
                        tBox.Add(prop);
                        
                        prop.RegisterCallback<ChangeEvent<UnityEngine.Object>>((chgEvent) => LoadPreviewImage(t.Icon)); //May calls but can't do otherwise yet
                    }
                    
                    LoadPreviewImage(t.Icon);
                }
            };
        }

        private void LoadPreviewImage(Sprite sprite)
        {
            var image = rootVisualElement.Q<Image>("preview-image");
            image.image = sprite == null ? null :sprite.texture;
        }

        /// <summary>
        /// Gather all assets of type T from a given relative path and sort them alphabetically
        /// </summary>
        /// <param name="relativePath">The relative path in Resources folder to scrap them</param>
        /// <param name="allT">The output array</param>
        private static void FindAllTFromResources(string relativePath, out T[] allT)
        {
            allT = Resources.LoadAll<T>(relativePath);
            
            //Sort alphabetically
            Array.Sort(allT, (a,b) => string.Compare(a.Title, b.Title, StringComparison.Ordinal));
            
            Debug.Log($"Loaded {allT.Length} {typeof(T).Name} from {relativePath}");
        }
    }
}

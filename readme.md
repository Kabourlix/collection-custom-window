# Collection Custom Window

A little package to easily create custom editor window of a list of serialized objects.

## How to

To create a new window for a collection of serialized objects to tracks, do the following : 
- Create a scriptable object that implement `ICollectableForWindowEditor` ;
- Create your window in an Editor folder by inheriting from `CollectionWindow<T>`, where `T` is the type of the scriptable object (SO) you have just created ;
- In your window class you should implements a static `ShowWindow` method (to create a button in the editor with MenuItem property) and `OnEnable`;
- In `OnEnable` you should define values for both strings `_resourcesRelativePath` (name of the folder, in Resources, where your SO will be stored) and `_resourcesPath` (localization of your Resources folder from Asset) ;
- Regarding Assembly Definition files : your Editor folder shall make reference to both assemblies of the package (runtime and editor).

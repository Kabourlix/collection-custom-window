using System;
using UnityEngine;

namespace GitHubProject.collection_custom_window.Runtime
{
    public interface ICollectableForWindowEditor
    {
        public string Title { get; set; }
        public Sprite Icon { get;}

    }
}
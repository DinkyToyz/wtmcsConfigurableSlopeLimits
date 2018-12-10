using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// User interface helper.
    /// </summary>
    internal class UI
    {
        /// <summary>
        /// The components.
        /// </summary>
        public readonly ComponentList Components;

        /// <summary>
        /// The parent child possession list.
        /// </summary>
        private static bool[] hasParentChild = new bool[] { false, true };

        /// <summary>
        /// The UI root.
        /// </summary>
        private UIView uiRoot = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="UI"/> class.
        /// </summary>
        public UI()
        {
            this.Components = new ComponentList(this);
        }

        /// <summary>
        /// Direction in which to do recursive logging of components.
        /// </summary>
        private enum LogDirection
        {
            /// <summary>
            /// Initial call.
            /// </summary>
            Init = 0,

            /// <summary>
            /// Logging parents.
            /// </summary>
            Parents = 1,

            /// <summary>
            /// Logging children.
            /// </summary>
            Children = 2
        }

        /// <summary>
        /// Gets the root.
        /// </summary>
        /// <value>
        /// The root.
        /// </value>
        protected UIView Root
        {
            get
            {
                if (this.uiRoot == null)
                {
                    Log.Debug(this, "Root", "Initialize");
                    try
                    {
                        foreach (UIView view in UIView.FindObjectsOfType<UIView>())
                        {
                            if (view.transform.parent == null && view.name == "UIView")
                            {
                                Log.Debug(this, "Root", "Initialized");
                                this.uiRoot = view;
                                break;
                            }
                        }

                        if (this.uiRoot == null)
                        {
                            Log.Debug(this, "Root", "Not found");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(this, "Root", ex);
                    }
                }

                return this.uiRoot;
            }
        }

        /// <summary>
        /// Logs the component data.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="block">The block.</param>
        /// <param name="component">The component.</param>
        /// <param name="componentName">Name of the component.</param>
        /// <param name="connectedName">Name of the connected.</param>
        /// <param name="logChildren">If set to <c>true</c> log children.</param>
        /// <param name="logParents">If set to <c>true</c> log parent.</param>
        public static void LogComponent(object source, string block, UIComponent component, string componentName = null, string connectedName = null, bool logChildren = true, bool logParents = false)
        {
            LogComponent(source, block, component, componentName, connectedName, logChildren, logParents, 0, LogDirection.Init);
        }

        /// <summary>
        /// Creates the atlas.
        /// </summary>
        /// <param name="atlasName">Name of the atlas.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="imageNames">The image names.</param>
        /// <param name="imageSize">Size of the image.</param>
        /// <param name="baseMaterial">The base material.</param>
        /// <returns>THe atlas.</returns>
        public UITextureAtlas CreateAtlas(string atlasName, string resourceName, string[] imageNames, int imageSize, Material baseMaterial)
        {
            try
            {
                Texture2D texture = new Texture2D(imageSize * imageNames.Length, imageSize, TextureFormat.ARGB32, false);
                texture.filterMode = FilterMode.Bilinear;

                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(this.GetType().Namespace + "." + resourceName))
                {
                    byte[] sprite = new byte[stream.Length];
                    stream.Read(sprite, 0, sprite.Length);
                    texture.LoadImage(sprite);
                }
                texture.Apply(true, true);

                Material material = Material.Instantiate<Material>(baseMaterial);
                material.mainTexture = texture;

                UITextureAtlas atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
                atlas.material = material;
                atlas.name = atlasName;

                float uw = 1.0f / imageNames.Length;
                for (int i = 0; i < imageNames.Length; i++)
                {
                    atlas.AddSprite(new UITextureAtlas.SpriteInfo()
                    {
                        name = imageNames[i],
                        texture = texture,
                        region = new Rect(i * uw, 0, uw, 1)
                    });
                }

                return atlas;
            }
            catch (Exception ex)
            {
                Log.Error(this, "CreateAtlas", ex, resourceName, imageSize, imageNames.Length);
                throw;
            }
        }

        /// <summary>
        /// Uninitialize this instance.
        /// </summary>
        public void DeInitialize()
        {
            Log.Debug(this, "DeInitialize", "Begin");

            this.Components.Clear();

            Log.Debug(this, "DeInitialize", "End");
        }

        /// <summary>
        /// Finds the component.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="name">The name.</param>
        /// <param name="parentComponent">The parent.</param>
        /// <returns>
        /// The component.
        /// </returns>
        public T FindComponent<T>(string name, UIComponent parentComponent = null) where T : UIComponent
        {
            ComponentList tmpcmps = new ComponentList(this, name, typeof(T), null, parentComponent);
            this.FindComponents(tmpcmps);
            return (T)tmpcmps[name];
        }

        /// <summary>
        /// Finds the components.
        /// </summary>
        public void FindComponents()
        {
            this.FindComponents(this.Components);
        }

        /// <summary>
        /// Finds the components.
        /// </summary>
        /// <param name="components">The components.</param>
        public void FindComponents(ComponentList components)
        {
            ////if (Log.LogToFile) Log.Debug(this, "FindComponents", "Begin");

            try
            {
                Dictionary<bool, HashSet<Type>> types = new Dictionary<bool, HashSet<Type>>() { { true, new HashSet<Type>() }, { false, new HashSet<Type>() } };
                foreach (ComponentInfo info in components)
                {
                    if (info.ParentName != null && info.ParentComponent == null)
                    {
                        info.ParentComponent = components[info.ParentName];
                    }

                    if (info.Component == null)
                    {
                        ////if (Log.LogToFile) Log.Debug(this, "FindComponents", "Types", info.Type, info.PathName);
                        types[info.ParentName != null].Add(info.Type);
                    }
                }

                foreach (bool hasParent in hasParentChild)
                {
                    bool found = false;
                    foreach (Type type in types[hasParent])
                    {
                        if (type != typeof(UIComponent) && types[hasParent].Contains(typeof(UIComponent)))
                        {
                            continue;
                        }

                        ////if (Log.LogToFile) Log.Debug(this, "FindComponents", "Type", type, hasParent);

                        foreach (UIComponent component in UIComponent.FindObjectsOfType(type))
                        {
                            foreach (ComponentInfo info in components)
                            {
                                if (info.Component == null && (info.Type == typeof(UIComponent) || info.Type == type) &&
                                    (hasParent == (info.ParentName != null)) &&
                                    (info.ParentName == null || info.ParentComponent != null) &&
                                    (component.name == info.Name))
                                {
                                    ////if (Log.LogToFile) Log.Debug(this, "FindComponents", "Component", type, info.PathName, component.GetType(), component.name);

                                    Transform topTrans = (info.ParentComponent == null) ? this.Root.transform : info.ParentComponent.transform;

                                    Transform curTrans = component.transform.parent;
                                    while (curTrans != null && curTrans != topTrans)
                                    {
                                        curTrans = curTrans.parent;
                                    }

                                    if (curTrans != null)
                                    {
                                        ////if (Log.LogToFile) Log.Debug(this, "FindComponents", "Found", type, info.PathName);

                                        if (info.Component == null)
                                        {
                                            info.Component = component;
                                            found = true;
                                        }
                                        else
                                        {
                                            info.Ambiguous = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (found && !hasParent)
                    {
                        foreach (ComponentInfo info in components)
                        {
                            if (info.ParentName != null && info.ParentComponent == null)
                            {
                                info.ParentComponent = components[info.ParentName];
                            }
                        }
                    }

                    if (Log.LogToFile)
                    {
                        foreach (ComponentInfo info in components)
                        {
                            if (hasParent == (info.ParentName != null) && info.Component == null)
                            {
                                if (info.ParentName != null && info.ParentComponent == null)
                                {
                                    ////if (Log.LogToFile) Log.Debug(this, "FindComponents", "No Parent", info.Type, info.PathName);
                                }
                                else
                                {
                                    ////if (Log.LogToFile) Log.Debug(this, "FindComponents", "Not Found", info.Type, info.PathName);
                                }
                            }
                        }
                    }
                }

                ////if (Log.LogToFile) Log.Debug(this, "FindComponents", "End");
            }
            catch (Exception ex)
            {
                Log.Error(this, "FindComponents", ex);
            }
        }

        /// <summary>
        /// Logs the components data.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="block">The block.</param>
        /// <param name="component">The component.</param>
        /// <param name="componentName">Name of the component.</param>
        /// <param name="connectedName">Name of the connected component.</param>
        /// <param name="logChildren">If set to <c>true</c> log children.</param>
        /// <param name="logParents">If set to <c>true</c> log parent.</param>
        /// <param name="depth">The recursion depth.</param>
        /// <param name="direction">The recursion direction.</param>
        private static void LogComponent(object source, string block, UIComponent component, string componentName, string connectedName, bool logChildren, bool logParents, int depth, LogDirection direction)
        {
            string componentPath = null;

            try
            {
                if (component != null && component is UIComponent)
                {
                    Log.InfoList info = new Log.InfoList();

                    componentPath = componentName;

                    if (String.IsNullOrEmpty(componentPath))
                    {
                        componentPath = component.cachedName;

                        if (String.IsNullOrEmpty(componentPath))
                        {
                            componentPath = "?";
                        }
                    }

                    if (!String.IsNullOrEmpty(connectedName))
                    {
                        componentPath = connectedName + "/" + componentPath;
                    }

                    try
                    {
                        foreach (var property in component.GetType().GetProperties())
                        {
                            if (property != null)
                            {
                                try
                                {
                                    info.Add(property.Name, property.GetValue(component, null));
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }

                    Log.Debug(source, block, depth, componentPath, component.GetType(), component, info);

                    if (depth < 32)
                    {
                        depth++;

                        if (logChildren && (direction == LogDirection.Init || direction == LogDirection.Children))
                        {
                            foreach (UIComponent child in component.components)
                            {
                                if (child != null)
                                {
                                    LogComponent(source, block, child, null, componentPath, logChildren, logParents, depth, LogDirection.Children);
                                }
                            }
                        }

                        if (logParents && (direction == LogDirection.Init || direction == LogDirection.Parents))
                        {
                            if (component.parent != null)
                            {
                                LogComponent(source, block, component.parent, "..", componentPath, logChildren, logParents, depth, LogDirection.Parents);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (String.IsNullOrEmpty(componentPath))
                {
                    Log.Debug(source, block, connectedName, componentName, ex.GetType(), ex.Message);
                }
                else
                {
                    Log.Debug(source, block, componentPath, ex.GetType(), ex.Message);
                }
            }
        }

        /// <summary>
        /// Component info holder.
        /// </summary>
        public class ComponentInfo
        {
            /// <summary>
            /// Component could not be uniquely found.
            /// </summary>
            public bool Ambiguous;

            /// <summary>
            /// The component.
            /// </summary>
            public UIComponent Component;

            /// <summary>
            /// The parent component.
            /// </summary>
            public UIComponent ParentComponent;

            /// <summary>
            /// Initializes a new instance of the <see cref="ComponentInfo"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="type">The type.</param>
            /// <param name="parentName">Name of the parent.</param>
            public ComponentInfo(string name, Type type, string parentName = null)
            {
                this.Initialize(name, type, parentName, null);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ComponentInfo"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="parentName">Name of the parent.</param>
            public ComponentInfo(string name, string parentName = null)
            {
                this.Initialize(name, typeof(UIComponent), parentName, null);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ComponentInfo"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="type">The type.</param>
            /// <param name="parentComponent">The parent component.</param>
            public ComponentInfo(string name, Type type, UIComponent parentComponent)
            {
                this.Initialize(name, type, null, parentComponent);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ComponentInfo"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="parentComponent">The parent component.</param>
            public ComponentInfo(string name, UIComponent parentComponent)
            {
                this.Initialize(name, typeof(UIComponent), null, parentComponent);
            }

            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the name of the parent.
            /// </summary>
            /// <value>
            /// The name of the parent.
            /// </value>
            public string ParentName
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the path name.
            /// </summary>
            /// <value>
            /// The path name.
            /// </value>
            public string PathName
            {
                get
                {
                    return (this.ParentName != null ? this.ParentName + "/" : this.ParentComponent != null ? this.ParentComponent.ToString() : "") + this.Name;
                }
            }

            /// <summary>
            /// Gets the type.
            /// </summary>
            /// <value>
            /// The type.
            /// </value>
            public Type Type
            {
                get;
                private set;
            }

            /// <summary>
            /// Initializes the properties.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="type">The type.</param>
            /// <param name="parentName">Name of the parent.</param>
            /// <param name="parentComponent">The parent component.</param>
            private void Initialize(string name, Type type, string parentName, UIComponent parentComponent)
            {
                this.Name = name;
                this.Type = type;
                this.ParentName = parentName;
                this.Component = null;
                this.ParentComponent = parentComponent;
                this.Ambiguous = false;
            }
        }

        /// <summary>
        /// A list of components.
        /// </summary>
        public class ComponentList : IEnumerable<ComponentInfo>
        {
            /// <summary>
            /// The components.
            /// </summary>
            private Dictionary<string, ComponentInfo> components = new Dictionary<string, ComponentInfo>();

            /// <summary>
            /// The UI.
            /// </summary>
            private UI ui = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="ComponentList"/> class.
            /// </summary>
            /// <param name="ui">The UI.</param>
            public ComponentList(UI ui)
            {
                this.Initialize(ui, null, typeof(UIComponent), null, null);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ComponentList"/> class and adds one component.
            /// </summary>
            /// <param name="ui">The UI.</param>
            /// <param name="name">The name.</param>
            /// <param name="parentName">Name of the parent.</param>
            /// <param name="parentComponent">The parent component.</param>
            public ComponentList(UI ui, string name, string parentName = null, UIComponent parentComponent = null)
            {
                this.Initialize(ui, name, typeof(UIComponent), parentName, parentComponent);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ComponentList"/> class and adds one component.
            /// </summary>
            /// <param name="ui">The UI.</param>
            /// <param name="name">The name.</param>
            /// <param name="type">The type.</param>
            /// <param name="parentName">Name of the parent.</param>
            /// <param name="parentComponent">The parent component.</param>
            public ComponentList(UI ui, string name, Type type, string parentName = null, UIComponent parentComponent = null)
            {
                this.Initialize(ui, name, type, parentName, parentComponent);
            }

            /// <summary>
            /// Gets or sets the <see cref="UIComponent"/> with the specified name.
            /// </summary>
            /// <value>
            /// The <see cref="UIComponent"/>.
            /// </value>
            /// <param name="name">The name.</param>
            /// <returns>The component control.</returns>
            public UIComponent this[string name]
            {
                get
                {
                    ComponentInfo info;
                    if (!this.components.ContainsKey(name))
                    {
                        info = new ComponentInfo(name);
                        this.components[name] = info;
                    }
                    else
                    {
                        info = this.components[name];
                    }

                    return info.Ambiguous ? (UIComponent)null : info.Component;
                }

                set
                {
                    if (!this.components.ContainsKey(name))
                    {
                        this.components[name] = new ComponentInfo(name);
                    }

                    this.components[name].Component = value;
                }
            }

            /// <summary>
            /// Adds the specified component.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="type">The type.</param>
            /// <param name="parentName">Name of the parent.</param>
            /// <param name="parentComponent">The parent component.</param>
            public void Add(string name, Type type, string parentName = null, UIComponent parentComponent = null)
            {
                string pathName = (parentName == null ? "" : parentName + "/") + name;
                if (!this.components.ContainsKey(pathName))
                {
                    this.components[pathName] = new ComponentInfo(name, type, parentName);
                }
            }

            /// <summary>
            /// Adds the specified component.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="parentName">Name of the parent.</param>
            /// <param name="parentComponent">The parent component.</param>
            public void Add(string name, string parentName = null, UIComponent parentComponent = null)
            {
                this.Add(name, typeof(UIComponent), parentName);
            }

            /// <summary>
            /// Clears this instance.
            /// </summary>
            public void Clear()
            {
                this.components.Clear();
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<ComponentInfo> GetEnumerator()
            {
                return this.components.Values.GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
            /// </returns>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.components.Values.GetEnumerator();
            }

            /// <summary>
            /// Initializes the properties.
            /// </summary>
            /// <param name="ui">The UI.</param>
            /// <param name="name">The name.</param>
            /// <param name="type">The type.</param>
            /// <param name="parentName">Name of the parent.</param>
            /// <param name="parentComponent">The parent component.</param>
            private void Initialize(UI ui, string name, Type type, string parentName, UIComponent parentComponent)
            {
                this.ui = ui;

                if (name != null)
                {
                    this.Add(name, type, parentName);
                }
            }
        }

        /// <summary>
        /// Set of sprites names for multi state buttons.
        /// </summary>
        public class MultiStateButtonSpriteSet
        {
            /// <summary>
            /// The sprite name for the disabled state.
            /// </summary>
            public string Disabled;

            /// <summary>
            /// The sprite name for the focused state.
            /// </summary>
            public string Focused;

            /// <summary>
            /// The sprite name for the hovered state.
            /// </summary>
            public string Hovered;

            /// <summary>
            /// The sprite name for the normal state.
            /// </summary>
            public string Normal;

            /// <summary>
            /// The sprite name for the pressed state.
            /// </summary>
            public string Pressed;

            /// <summary>
            /// Initializes a new instance of the <see cref="MultiStateButtonSpriteSet" /> class.
            /// </summary>
            public MultiStateButtonSpriteSet()
            {
                this.Normal = null;
                this.Disabled = null;
                this.Focused = null;
                this.Hovered = null;
                this.Pressed = null;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="MultiStateButtonSpriteSet"/> class.
            /// </summary>
            /// <param name="normal">The sprite name for the normal state.</param>
            /// <param name="disabled">The sprite name for the disabled state.</param>
            /// <param name="focused">The sprite name for the focused state.</param>
            /// <param name="hovered">The sprite name for the hovered state.</param>
            /// <param name="pressed">The sprite name for the pressed state.</param>
            public MultiStateButtonSpriteSet(string normal, string disabled, string focused, string hovered, string pressed)
            {
                this.Normal = normal;
                this.Disabled = disabled;
                this.Focused = focused;
                this.Hovered = hovered;
                this.Pressed = pressed;
            }
        }

        /// <summary>
        /// Sets of fore- and background sprites for multistate buttons.
        /// </summary>
        public class MultiStateButtonSpriteSets
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MultiStateButtonSpriteSets"/> class.
            /// </summary>
            public MultiStateButtonSpriteSets()
            {
                this.Background = new MultiStateButtonSpriteSet();
                this.Foreground = new MultiStateButtonSpriteSet();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="MultiStateButtonSpriteSets"/> class.
            /// </summary>
            /// <param name="background">The background sprites.</param>
            /// <param name="foreground">The foreground sprites.</param>
            public MultiStateButtonSpriteSets(MultiStateButtonSpriteSet background, MultiStateButtonSpriteSet foreground)
            {
                this.Background = background;
                this.Foreground = foreground;
            }

            /// <summary>
            /// Gets the background sprites.
            /// </summary>
            public MultiStateButtonSpriteSet Background
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the foreground sprites.
            /// </summary>
            public MultiStateButtonSpriteSet Foreground
            {
                get;
                private set;
            }
        }

        /// <summary>
        /// List of sets of fore- and background sprites for multi state buttons.
        /// </summary>
        public class MultiStateButtonSpriteSetsList
        {
            /// <summary>
            /// The list.
            /// </summary>
            private MultiStateButtonSpriteSets[] list = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="MultiStateButtonSpriteSetsList"/> class.
            /// </summary>
            public MultiStateButtonSpriteSetsList()
            {
                this.list = new MultiStateButtonSpriteSets[0];
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="MultiStateButtonSpriteSetsList"/> class.
            /// </summary>
            /// <param name="disabled">The sprites for a disabled button.</param>
            /// <param name="enabled">The sprites for an enabled button.</param>
            public MultiStateButtonSpriteSetsList(MultiStateButtonSpriteSets disabled, MultiStateButtonSpriteSets enabled)
            {
                this.list = new MultiStateButtonSpriteSets[2];
                this.list[0] = disabled;
                this.list[1] = enabled;
            }

            /// <summary>
            /// Gets the length.
            /// </summary>
            /// <value>
            /// The length.
            /// </value>
            public int Length
            {
                get
                {
                    return (this.list == null) ? 0 : this.list.Length;
                }
            }

            /// <summary>
            /// Gets the <see cref="MultiStateButtonSpriteSets"/> at the specified index.
            /// </summary>
            /// <value>
            /// The <see cref="MultiStateButtonSpriteSets"/>.
            /// </value>
            /// <param name="index">The index.</param>
            /// <returns>The sets of fore- and background sprites.</returns>
            public MultiStateButtonSpriteSets this[int index]
            {
                get
                {
                    return this.list[index];
                }
            }

            /// <summary>
            /// Applies the sprites.
            /// </summary>
            /// <param name="button">The button.</param>
            public void Apply(UIMultiStateButton button)
            {
                for (int i = 0; i < this.list.Length; i++)
                {
                    try
                    {
                        if (this.list[i].Background == null)
                        {
                            throw new NullReferenceException("[" + i.ToString() + "].Background == null");
                        }

                        if (this.list[i].Foreground == null)
                        {
                            throw new NullReferenceException("[" + i.ToString() + "].Foreground == null");
                        }

                        while (i >= button.backgroundSprites.Count)
                        {
                            button.backgroundSprites.AddState();
                        }

                        while (i >= button.foregroundSprites.Count)
                        {
                            button.foregroundSprites.AddState();
                        }

                        button.backgroundSprites[i].disabled = this.list[i].Background.Disabled;
                        button.backgroundSprites[i].focused = this.list[i].Background.Focused;
                        button.backgroundSprites[i].hovered = this.list[i].Background.Hovered;
                        button.backgroundSprites[i].normal = this.list[i].Background.Normal;
                        button.backgroundSprites[i].pressed = this.list[i].Background.Pressed;

                        button.foregroundSprites[i].disabled = this.list[i].Foreground.Disabled;
                        button.foregroundSprites[i].focused = this.list[i].Foreground.Focused;
                        button.foregroundSprites[i].hovered = this.list[i].Foreground.Hovered;
                        button.foregroundSprites[i].normal = this.list[i].Foreground.Normal;
                        button.foregroundSprites[i].pressed = this.list[i].Foreground.Pressed;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(this, "ApplySprites", ex, i, this.list.Length);
                        throw;
                    }
                }
            }
        }
    }
}
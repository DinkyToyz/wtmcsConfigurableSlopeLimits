﻿using ColossalFramework.UI;
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
        /// The components
        /// </summary>
        public readonly ComponentList Components;

        /// <summary>
        /// The parent child possesion list.
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
            Components = new ComponentList(this);
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
                if (uiRoot == null)
                {
                    Log.Debug(this, "Root", "Initialize");
                    try
                    {
                        foreach (UIView view in UIView.FindObjectsOfType<UIView>())
                        {
                            if (view.transform.parent == null && view.name == "UIView")
                            {
                                Log.Debug(this, "Root", "Initialized");
                                uiRoot = view;
                                break;
                            }
                        }

                        if (uiRoot == null)
                        {
                            Log.Debug(this, "Root", "Not found");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(this, "Root", ex);
                    }
                }

                return uiRoot;
            }
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
                    atlas.AddSprite(new UITextureAtlas.SpriteInfo() { name = imageNames[i], texture = texture, region = new Rect(i * uw, 0, uw, 1) });
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
        /// Deinitialize this instance.
        /// </summary>
        public void DeInitialize()
        {
            Log.Debug(this, "DeInitialize", "Begin");

            Components.Clear();

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
            FindComponents(tmpcmps);
            return (T)(tmpcmps[name]);
        }

        /// <summary>
        /// Finds the components.
        /// </summary>
        public void FindComponents()
        {
            FindComponents(Components);
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

                                    Transform tTrans = (info.ParentComponent == null) ? Root.transform : info.ParentComponent.transform;

                                    Transform pTrans = component.transform.parent;
                                    while (pTrans != null && pTrans != tTrans)
                                    {
                                        pTrans = pTrans.parent;
                                    }

                                    if (pTrans != null)
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
            /// The parent component
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
                initialize(name, type, parentName, null);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ComponentInfo"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="parentName">Name of the parent.</param>
            public ComponentInfo(string name, string parentName = null)
            {
                initialize(name, typeof(UIComponent), parentName, null);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ComponentInfo"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="type">The type.</param>
            /// <param name="parentComponent">The parent component.</param>
            public ComponentInfo(string name, Type type, UIComponent parentComponent)
            {
                initialize(name, type, null, parentComponent);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ComponentInfo"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="parentComponent">The parent component.</param>
            public ComponentInfo(string name, UIComponent parentComponent)
            {
                initialize(name, typeof(UIComponent), null, parentComponent);
            }

            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; private set; }

            /// <summary>
            /// Gets the name of the parent.
            /// </summary>
            /// <value>
            /// The name of the parent.
            /// </value>
            public string ParentName { get; private set; }

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
                    return (ParentName != null ? ParentName + "/" : ParentComponent != null ? ParentComponent.ToString() : "") + Name;
                }
            }

            /// <summary>
            /// Gets the type.
            /// </summary>
            /// <value>
            /// The type.
            /// </value>
            public Type Type { get; private set; }

            /// <summary>
            /// Initializes the properties.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="type">The type.</param>
            /// <param name="parentName">Name of the parent.</param>
            /// <param name="parentComponent">The parent component.</param>
            private void initialize(string name, Type type, string parentName, UIComponent parentComponent)
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
                initialize(ui, null, typeof(UIComponent), null, null);
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
                initialize(ui, name, typeof(UIComponent), parentName, parentComponent);
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
                initialize(ui, name, type, parentName, parentComponent);
            }

            /// <summary>
            /// Gets or sets the <see cref="UIComponent"/> with the specified name.
            /// </summary>
            /// <value>
            /// The <see cref="UIComponent"/>.
            /// </value>
            /// <param name="name">The name.</param>
            /// <returns></returns>
            public UIComponent this[string name]
            {
                get
                {
                    ComponentInfo info;
                    if (!components.ContainsKey(name))
                    {
                        info = new ComponentInfo(name);
                        components[name] = info;
                    }
                    else
                    {
                        info = components[name];
                    }

                    return info.Ambiguous ? (UIComponent)null : info.Component;
                }

                set
                {
                    if (!components.ContainsKey(name))
                    {
                        components[name] = new ComponentInfo(name);
                    }

                    components[name].Component = value;
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
                if (!components.ContainsKey(pathName))
                {
                    components[pathName] = new ComponentInfo(name, type, parentName);
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
                Add(name, typeof(UIComponent), parentName);
            }

            /// <summary>
            /// Clears this instance.
            /// </summary>
            public void Clear()
            {
                components.Clear();
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<ComponentInfo> GetEnumerator()
            {
                return components.Values.GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
            /// </returns>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return components.Values.GetEnumerator();
            }

            /// <summary>
            /// Initializes the properties.
            /// </summary>
            /// <param name="ui">The UI.</param>
            /// <param name="name">The name.</param>
            /// <param name="type">The type.</param>
            /// <param name="parentName">Name of the parent.</param>
            /// <param name="parentComponent">The parent component.</param>
            private void initialize(UI ui, string name, Type type, string parentName, UIComponent parentComponent)
            {
                this.ui = ui;

                if (name != null)
                {
                    Add(name, type, parentName);
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
        /// Sets of fore- and backround sprites for multistate buttons.
        /// </summary>
        public class MultiStateButtonSpriteSets
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MultiStateButtonSpriteSets"/> class.
            /// </summary>
            public MultiStateButtonSpriteSets()
            {
                Background = new MultiStateButtonSpriteSet();
                Foreground = new MultiStateButtonSpriteSet();
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
            /// The background sprites.
            /// </summary>
            public MultiStateButtonSpriteSet Background { get; private set; }

            /// <summary>
            /// The foreground sprites.
            /// </summary>
            public MultiStateButtonSpriteSet Foreground { get; private set; }
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
                list = new MultiStateButtonSpriteSets[0];
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="MultiStateButtonSpriteSetsList"/> class.
            /// </summary>
            /// <param name="disabled">The sprites for a disabled button.</param>
            /// <param name="enabled">The sprites for an enabled button.</param>
            public MultiStateButtonSpriteSetsList(MultiStateButtonSpriteSets disabled, MultiStateButtonSpriteSets enabled)
            {
                list = new MultiStateButtonSpriteSets[2];
                list[0] = disabled;
                list[1] = enabled;
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
                    return (list == null) ? 0 : list.Length;
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
                    return list[index];
                }
            }

            /// <summary>
            /// Applies the sprites.
            /// </summary>
            /// <param name="button">The button.</param>
            public void Apply(UIMultiStateButton button)
            {
                for (int i = 0; i < list.Length; i++)
                {
                    try
                    {
                        if (list[i].Background == null)
                        {
                            throw new NullReferenceException("[" + i.ToString() + "].Background == null");
                        }

                        if (list[i].Foreground == null)
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

                        button.backgroundSprites[i].disabled = list[i].Background.Disabled;
                        button.backgroundSprites[i].focused = list[i].Background.Focused;
                        button.backgroundSprites[i].hovered = list[i].Background.Hovered;
                        button.backgroundSprites[i].normal = list[i].Background.Normal;
                        button.backgroundSprites[i].pressed = list[i].Background.Pressed;

                        button.foregroundSprites[i].disabled = list[i].Foreground.Disabled;
                        button.foregroundSprites[i].focused = list[i].Foreground.Focused;
                        button.foregroundSprites[i].hovered = list[i].Foreground.Hovered;
                        button.foregroundSprites[i].normal = list[i].Foreground.Normal;
                        button.foregroundSprites[i].pressed = list[i].Foreground.Pressed;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(this, "ApplySprites", ex, i, list.Length);
                        throw;
                    }
                }
            }
        }
    }
}
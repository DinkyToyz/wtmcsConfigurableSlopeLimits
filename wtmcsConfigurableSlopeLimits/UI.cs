using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// User interface helper.
    /// </summary>
    internal class UI
    {
        public readonly ComponentList Components;

        public UI()
        {
            Components = new ComponentList(this);
        }

        /// <summary>
        /// The main tool strip.
        /// </summary>
        private UITabstrip mainToolStrip = null;

        /// <summary>
        /// The UI root.
        /// </summary>
        private UIView uiRoot = null;

        /// <summary>
        /// Gets the main tool strip.
        /// </summary>
        /// <value>
        /// The main tool strip.
        /// </value>
        public UITabstrip MainToolStrip
        {
            get
            {
                if (mainToolStrip == null)
                {
                    mainToolStrip = FindComponent<UITabstrip>("MainToolstrip");
                }

                return mainToolStrip;
            }
        }

        public class ComponentList
        {
            private UI ui = null;

            public ComponentList(UI ui)
            {
                this.ui = ui; 
            }

            Dictionary<string, UIComponent> components = new Dictionary<string, UIComponent>();

            public UIComponent this[string name]
            {
                get
                {
                    if (!components.ContainsKey(name))
                    {
                        UIComponent component = ui.FindComponent<UIComponent>(name);
                        if (component == null)
                        {
                            return null;
                        }

                        components[name] = component;
                    }

                    return components[name];
                }
            }

            public void Clear()
            {
                components.Clear();
            }
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
        /// Get the built-in tabstrip.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns>The tabstrip.</returns>
        public UITabstrip BuiltInTabsstrip(UIComponent parent)
        {
            return FindComponent<UITabstrip>("ToolMode", parent);
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
        /// <param name="parent">The parent.</param>
        /// <param name="exactMatch">If set to <c>true</c> name must be exact match.</param>
        /// <returns>The component.</returns>
        public T FindComponent<T>(string name, UIComponent parent = null, bool exactMatch = false) where T : UIComponent
        {
            List<T> components = FindComponents<T>(name, parent, exactMatch, 1);

            return (components == null || components.Count != 1) ? (T)null : components[0];
        }
        /// <summary>
        /// Finds the component.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="exactMatch">If set to <c>true</c> name must be exact match.</param>
        /// <returns>
        /// The component.
        /// </returns>
        public UIComponent FindComponent(string name, UIComponent parent = null, bool exactMatch = false)
        {
            return FindComponent<UIComponent>(name, parent, exactMatch);
        }

        /// <summary>
        /// Finds the components.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="exactMatch">If set to <c>true</c> name must be exact match.</param>
        /// <param name="maxCount">The maximum numeber of components to return.</param>
        /// <returns>
        /// The components.
        /// </returns>
        public List<T> FindComponents<T>(string name, UIComponent parent = null, bool exactMatch = false, int? maxCount = null) where T : UIComponent
        {
            //Log.Debug(this, "FindComponents", "'" + name + "'");

            List<T> components = new List<T>();

            string lcName = exactMatch ? null : name.ToLowerInvariant();

            try
            {
                foreach (T component in UIComponent.FindObjectsOfType<T>())
                {
                    if (component.name == name || (!exactMatch && component.name.ToLowerInvariant().Contains(lcName)))
                    {
                        Transform tTrans = (parent == null) ? Root.transform : parent.transform;

                        Transform pTrans = component.transform.parent;
                        while (pTrans != null && pTrans != tTrans)
                        {
                            pTrans = pTrans.parent;
                        }

                        if (pTrans != null)
                        {
                            //Log.Debug(this, "FindComponents", "Found", component.name);
                            components.Add(component);

                            if (maxCount != null && maxCount.HasValue && components.Count >= maxCount.Value)
                            {
                                break;
                            }
                        }
                    }
                }

                //if (components.Count == 0)
                //{
                //    Log.Debug(this, "FindComponents", "Not found");
                //}

                return components;
            }
            catch (Exception ex)
            {
                Log.Error(this, "FindComponents", ex);
                return null;
            }
        }

        /// <summary>
        /// Finds the components.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="exactMatch">If set to <c>true</c> name must be exact match.</param>
        /// <param name="maxCount">The maximum numeber of components to return.</param>
        /// <returns>
        /// The components.
        /// </returns>
        public List<UIComponent> FindComponents(string name, UIComponent parent = null, bool exactMatch = false, int? maxCount = null)
        {
            return FindComponents<UIComponent>(name, parent, exactMatch, maxCount);
        }

        /// <summary>
        /// Gets the component paths.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="exactMatch">If set to <c>true</c> name must be exact match.</param>
        /// <returns>The component paths.</returns>
        public List<string> GetComponentPaths<T>(string name, UIComponent parent = null, bool exactMatch = false) where T : UIComponent
        {
            List<string> paths = new List<string>();

            try
            {
                foreach (T component in FindComponents<T>(name, parent, exactMatch))
                {
                    StringBuilder path = new StringBuilder();

                    Transform tTrans = (parent == null) ? Root.transform : parent.transform;

                    Transform pTrans = component.transform;
                    while (pTrans != null)
                    {
                        if (path.Length > 0)
                        {
                            path.Append('|');
                        }
                        if (pTrans == tTrans)
                        {
                            path.Append('<');
                        }
                        path.Append(pTrans.name);
                        if (pTrans == tTrans)
                        {
                            path.Append('>');
                        }

                        pTrans = pTrans.parent;
                    }

                    path.Insert(0, ": ").Insert(0, component.name);
                    paths.Add(path.ToString());
                }

                return paths;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the component paths.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="exactMatch">If set to <c>true</c> name must be exact match.</param>
        /// <returns>
        /// The component paths.
        /// </returns>
        public List<string> GetComponentPaths(string name, UIComponent parent = null, bool exactMatch = false)
        {
            return GetComponentPaths<UIComponent>(name, parent, exactMatch);
        }

        /// <summary>
        /// Logs the component paths.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="caller">The caller.</param>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="exactMatch">If set to <c>true</c> name must be exact match.</param>
        public void LogComponentPaths<T>(object caller, string name, UIComponent parent = null, bool exactMatch = false) where T : UIComponent
        {
            Log.Debug(this, "LogComponentPaths", "'" + name + "'");

            List<string> paths = GetComponentPaths<T>(name, parent, exactMatch);

            if (paths == null)
            {
                Log.Debug(caller, "LogComponentPaths", typeof(T).Name, name, "~");
            }
            else
            {
                HashSet<string> shown = new HashSet<string>();

                foreach (string path in paths)
                {
                    if (!shown.Contains(path))
                    {
                        Log.Debug(caller, "LogComponentPaths", typeof(T).Name, name, path);
                        shown.Add(path);
                    }
                }
            }

            Log.Debug(this, "LogComponentPaths");
        }

        /// <summary>
        /// Logs the component paths.
        /// </summary>
        /// <param name="caller">The caller.</param>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="exactMatch">If set to <c>true</c> name must be exact match.</param>
        public void LogComponentPaths(object caller, string name, UIComponent parent = null, bool exactMatch = false)
        {
            LogComponentPaths<UIComponent>(caller, name, parent, exactMatch);
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
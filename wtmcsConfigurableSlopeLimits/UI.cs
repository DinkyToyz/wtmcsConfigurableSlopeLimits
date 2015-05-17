using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ConfigurableSlopeLimits
{
    /// <summary>
    /// User interface helper.
    /// </summary>
    internal class UI
    {
        /// <summary>
        /// The roads option panel.
        /// </summary>
        private UIComponent roadsOptionPanel = null;

        /// <summary>
        /// The roads panel.
        /// </summary>
        private UIPanel roadsPanel = null;

        /// <summary>
        /// The UI root.
        /// </summary>
        private UIView uiRoot = null;

        /// <summary>
        /// Gets the roads option panel.
        /// </summary>
        /// <value>
        /// The roads option panel.
        /// </value>
        public UIComponent RoadsOptionPanel
        {
            get
            {
                if (roadsOptionPanel == null)
                {
                    try
                    {
                        roadsOptionPanel = FindComponent<UIComponent>("RoadsOptionPanel(RoadsPanel)");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(this, "RoadsOptionPanel", ex);
                    }
                }

                return roadsOptionPanel;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the roads option panel is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if the roads option panel is visible; otherwise, <c>false</c>.
        /// </value>
        public bool RoadsOptionPanelIsVisible
        {
            get
            {
                return (RoadsOptionPanel != null && RoadsOptionPanel.isVisible && RoadsOptionPanel.gameObject.activeInHierarchy);
            }
        }

        /// <summary>
        /// Gets the roads panel.
        /// </summary>
        /// <value>
        /// The roads panel.
        /// </value>
        public UIPanel RoadsPanel
        {
            get
            {
                if (roadsPanel == null)
                {
                    try
                    {
                        roadsPanel = UIView.Find<UIPanel>("RoadsPanel");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(this, "RoadsPanel", ex);
                    }
                }

                return roadsPanel;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the roads panel is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if the roads panel is visible; otherwise, <c>false</c>.
        /// </value>
        public bool RoadsPanelIsVisible
        {
            get
            {
                return (RoadsPanel != null && RoadsPanel.isVisible);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the roads tool is visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the roads tool is vsible; otherwise, <c>false</c>.
        /// </value>
        public bool RoadsToolIsVisible
        {
            get
            {
                return (RoadsPanelIsVisible && RoadsOptionPanelIsVisible);
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
        /// Deinitialize this instance.
        /// </summary>
        public void DeInitialize()
        {
            Log.Debug(this, "DeInitialize", "Begin");

            roadsPanel = null;
            roadsOptionPanel = null;

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
            Log.Debug(this, "Find", "'" + name + "'");

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
                            Log.Debug(this, "Find", "Found");
                            return component;
                        }
                    }
                }

                Log.Debug(this, "Find", "Not found");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(this, "Find", ex);
                return null;
            }
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
            string lcName = exactMatch ? null : name.ToLowerInvariant();
            List<string> paths = new List<string>();

            try
            {
                foreach (T component in UIComponent.FindObjectsOfType<T>())
                {
                    if (component.name == name || (!exactMatch && component.name.ToLowerInvariant().Contains(lcName)))
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
                }

                return paths;
            }
            catch
            {
                return null;
            }
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
            List<string> paths = GetComponentPaths<T>(name, parent, exactMatch);

            if (paths == null)
            {
                Log.Debug(caller, "LogComponentPaths", typeof(T).Name, name, "~");
            }
            else
            {
                foreach (string path in paths)
                {
                    Log.Debug(caller, "LogComponentPaths", typeof(T).Name, name, path);
                }
            }
        }
    }
}
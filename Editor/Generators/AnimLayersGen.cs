using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ConstGen
{
    [InitializeOnLoad]
    public class AnimLayersGen
    {
        #region Variables =================================================================================================================
        private const string FileName = "_ANIMLAYERS";
        
        /// <summary>
        /// An instance of the generator class itself
        /// it's main purpose is to instantiate and cache a ConstantGenerator as it's property
        /// to be used for generating the code
        /// </summary>
        private static AnimLayersGen instance;
        private ConstantGenerator generator_ = new ConstantGenerator();
        private static ConstantGenerator generator { get { return instance.generator_; } }
        private static string FilePath { get { return string.Format(ConstantGenerator.FilePathFormat, generator.GetOutputPath(), FileName); } }
        private bool regenerateOnMissing;
        private bool updateOnReload;
        private List<ConstGenSettings.LayersCTRLR> newLayers;
        private List<ConstGenSettings.LayersCTRLR> oldLayers;
        #endregion Variables ===============================================================================================================

        // static constructor
        // get's called with the class when [InitializeOnLoad] happens
        static AnimLayersGen()
        {
            CreateGeneratorInsance();
            RetrieveSettingsData();
                
            if ( !File.Exists(FilePath) ) // check if file exist
            {
                if ( instance.regenerateOnMissing )
                { // if allowed, generate a new since none is present
                    Debug.LogWarning( "[ " + FileName + " ] is not found, Generating a new one...");
                    Generate();
                } 
                else if ( instance.updateOnReload )
                { // log that generator tried to update a non existent file
                    Debug.LogWarning( "[ " + FileName + 
                        " ] Update generation failed because file is non-existent" );
                }
            } 
            else 
            {
                // file exist and check  if we can updateOnReload
                if ( instance.updateOnReload ) {
                    UpdateFile();
                }
            }
        }

        private static void CreateGeneratorInsance()
        {
            if (instance != null) return;

            // create self instance of the class
            // then create and cache the ConstantGenerator instance to it's property
            instance = new AnimLayersGen();
            instance.generator_ = new ConstantGenerator();
        }

        private static void RetrieveSettingsData()
        {
            ConstGenSettings cgs = ConstantGenerator.GetSettingsFile();

            instance.regenerateOnMissing = cgs.regenerateOnMissing; 
            instance.updateOnReload = cgs.updateOnReload;
            instance.oldLayers = cgs._ANIMLAYERS;
        }

        /// <summary>
        /// Generates the file by writing new updated contents or generates the file is none is present
        /// </summary>
        public static void Generate()
        {
            CreateGeneratorInsance();
            instance.newLayers = RetriveValues();

            // store the new properties to SO
            ConstantGenerator.GetSettingsFile()._ANIMLAYERS.Clear();
            ConstantGenerator.GetSettingsFile()._ANIMLAYERS = instance.newLayers;

            // set SO to be dirty to be saved
            EditorUtility.SetDirty( ConstantGenerator.GetSettingsFile() );

            GenerateCode(); 
        }

        /// <summary>
        /// Generators automatically generate their out files when not present
        /// so we can force them to generate it by deleting the file
        /// </summary>
        public static void ForceGenerate()
        {
            CreateGeneratorInsance();

            if ( File.Exists(FilePath) ) {
                AssetDatabase.DeleteAsset( FilePath );
                AssetDatabase.Refresh();
            }
            else 
            {
                Debug.LogWarning( "[ " + FileName + " ] Force Generate Failed, trying to delete an non existent file" );                
            }

        }

        /// <summary>
        /// checks if there is any changes on the constants 
        /// </summary>
        private static void UpdateFile()
        {
            if (Application.isPlaying) return;

            bool generate = false;
            instance.newLayers = RetriveValues();

            // check if the number of animation controllers in the assets has changed
            if ( instance.oldLayers.Count != instance.newLayers.Count ) {
                generate = true;
            } 
            else // else check for changes in the layers of the controllers
            { 
                // loop through animators
                for (int i = 0; i < instance.oldLayers.Count && generate == false; i++) {

                    ConstGenSettings.LayersCTRLR oldCTRLR = instance.oldLayers[i];
                    ConstGenSettings.LayersCTRLR newCTRLR = instance.newLayers[i];

                    // check if the name of the controller has changed or
                    // if any layers is added or removed
                    if ( oldCTRLR.name != newCTRLR.name || 
                        oldCTRLR.layers.Count != newCTRLR.layers.Count ) {

                        generate = true;
                        break;
                    }
                    else // else check if any of the name of layers has changed
                    {
                        // loop through layers
                        for (int i2 = 0; i2 < oldCTRLR.layers.Count; i2++)
                        {
                            string oldName = oldCTRLR.layers[i2];
                            string newName = newCTRLR.layers[i2];

                            // compare layer names
                            if ( oldName != newName ) { 
                                generate = true;
                                break;
                            }
                        }
                    }

                    if ( generate ) // break out animators loop
                        break;
                }
            }

            if ( generate ) {  
                Generate();
            }
        }

        private string GetScriptName()
        {
            StringBuilder name = new StringBuilder( this.ToString() );
            int dotIndex = name.ToString().IndexOf('.');
            name.Remove( 0, dotIndex+1 );

            return name.ToString();
        }

        static List<ConstGenSettings.LayersCTRLR> RetriveValues()
        {
            // find controller GUIDs and create LayersCTRLR list
            string[] controllers = AssetDatabase.FindAssets("t:animatorcontroller");
            List<ConstGenSettings.LayersCTRLR> layersControllers = new List<ConstGenSettings.LayersCTRLR>();

            foreach (string CTRLR in controllers)  
            {
                // get controller and it's path
                string path = AssetDatabase.GUIDToAssetPath(CTRLR);
                UnityEditor.Animations.AnimatorController animCTRLR = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path);
                
                if (animCTRLR.layers.Length == 0) continue;

                ConstGenSettings.LayersCTRLR layerCTRLR = new ConstGenSettings.LayersCTRLR();
                layerCTRLR.name = animCTRLR.name;

                // loop through controller's layers and cache it
                foreach (var layer_ in animCTRLR.layers)
                {
                    layerCTRLR.layers.Add( layer_.name );
                }

                layersControllers.Add( layerCTRLR );
            }

            return layersControllers;
        }

        private static void GenerateCode()
        {
            generator.GenerateCodeFile(FilePath, generator.GetOutputPath() ,content =>
            {
                WrappedInt indentCount = 0;

                // NOTE: indentCount is automatically increamented or decreamented by
                // ContentWriter methods 

                // An indent is added when a new CurlyBrackets IDisposable instance is created
                // and removed everytime that IDisposable instance is disposed of
                // (when the control has excited from the using statement)

                content.WriteIndentedLine( indentCount, generator.GetHeaderText( instance.GetScriptName() ) ); 
                content.WriteImports( "UnityEngine" );

                using (new CurlyBrackets(content, ConstantGenerator.OutputFileNamespace, indentCount))
                {
                    using (new CurlyBrackets(content, "public static class " + FileName, indentCount))
                    {
                        foreach (ConstGenSettings.LayersCTRLR ctrlr in instance.newLayers)
                        {
                            // write layers animator header group
                            content.WriteIndentedFormatLine(indentCount, "public static class {0}", generator.MakeIdentifier(ctrlr.name));
                            using (new CurlyBrackets(content, indentCount))
                            {
                                int layerIndex = 0;
                                foreach (string parameters in ctrlr.layers )
                                {
                                    content.WriteIndentedFormatLine(indentCount, 
                                        "public const int {0} = {1};", 
                                            generator.MakeIdentifier(parameters), layerIndex);
                                    
                                    layerIndex++;
                                }
                            }
                            content.WriteNewLine();
                        }
                    }                    
                }
            });
        }
    }
}






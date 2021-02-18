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
    public class AnimStatesGen
    {
        #region Variables =================================================================================================================
        private const string FileName = "_ANIMSTATES";
        
        /// <summary>
        /// An instance of the generator class itself
        /// it's main purpose is to instantiate and cache a ConstantGenerator as it's property
        /// to be used for generating the code
        /// </summary>
        private static AnimStatesGen instance;
        private ConstantGenerator generator_ = new ConstantGenerator();
        private static ConstantGenerator generator { get { return instance.generator_; } }
        private static string FilePath { get { return string.Format(ConstantGenerator.FilePathFormat, generator.GetOutputPath(), FileName); } }
        private bool regenerateOnMissing;
        private bool updateOnReload;
        private List<ConstGenSettings.StatesCTRLR> oldStateCTRLRS;
        private List<ConstGenSettings.StatesCTRLR> newStateCTRLRS;
        #endregion Variables ===============================================================================================================

        // static constructor
        // get's called with the class when [InitializeOnLoad] happens
        static AnimStatesGen()
        {
            CreateGeneratorInsance();

            if ( !RetrieveSettingsData() )
                return;
                
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
            instance = new AnimStatesGen();
            instance.generator_ = new ConstantGenerator();
        }

        private static bool RetrieveSettingsData()
        {
            bool successful = false;

            try
            {
                ConstGenSettings cgs = ConstantGenerator.GetSettingsFile();

                instance.regenerateOnMissing = cgs.regenerateOnMissing;
                instance.updateOnReload = cgs.updateOnReload;
                instance.oldStateCTRLRS = cgs._ANIMSTATES;

                successful = true;
            }
            catch (System.Exception)
            {
                successful = false;
                throw;
            }

            return successful;
        }

        /// <summary> 
        /// Generates the file by writing new updated contents or generates the file is none is present
        /// </summary>
        public static void Generate()
        {
            CreateGeneratorInsance();
            instance.newStateCTRLRS = RetriveValues();

            // store the new properties to SO
            ConstantGenerator.GetSettingsFile()._ANIMSTATES.Clear();
            ConstantGenerator.GetSettingsFile()._ANIMSTATES = instance.newStateCTRLRS;

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
                Debug.LogWarning( "[ " + FileName + " ] ForceGenerate Failed, trying to delete an non existent file" );                
            }

        }

        /// <summary>
        /// checks if there is any changes on the constants 
        /// </summary>
        private static void UpdateFile()
        {
            if (Application.isPlaying) return;

            bool generate = false;
            instance.newStateCTRLRS = RetriveValues();

            // check if the number of controllers has changed
            if ( instance.newStateCTRLRS.Count != instance.oldStateCTRLRS.Count )
            {
                generate = true;
            }
            else
            {
                // loop through animator controllers
                for (int i = 0; i < instance.oldStateCTRLRS.Count; i++)
                {
                    List<ConstGenSettings.AnimLayer> oldAnimLayers = instance.oldStateCTRLRS[i].animLayers;
                    List<ConstGenSettings.AnimLayer> newAnimLayers = instance.newStateCTRLRS[i].animLayers;

                    // check if the number of layers has changed
                    if ( oldAnimLayers.Count != newAnimLayers.Count ) {
                        generate = true;
                        break;
                    }

                    if ( !generate ) {
                        // loop through all the animator layers
                        for (int i2 = 0; i2 < oldAnimLayers.Count; i2++)
                        {
                            ConstGenSettings.AnimLayer oldLayer = oldAnimLayers[i2];
                            ConstGenSettings.AnimLayer newLayer = newAnimLayers[i2];

                            // compare layer names if it has changed
                            if ( oldLayer.name != newLayer.name ) {
                                generate = true;
                                break;
                            }

                            if ( !generate )  // check the anim states in the layer
                            {
                                List<ConstGenSettings.AnimState> oldStates = oldLayer.animStates;
                                List<ConstGenSettings.AnimState> newStates = newLayer.animStates;                                

                                // check if the number of states has changed
                                if ( oldStates.Count != newStates.Count ) {
                                    generate = true;
                                    break;
                                }

                                if ( !generate ) // loop through all states in that layer
                                { 
                                    for (int i3 = 0; i3 < oldStates.Count; i3++) 
                                    {
                                        ConstGenSettings.AnimState oldState = oldStates[i3];
                                        ConstGenSettings.AnimState newState = newStates[i3];

                                        // compare names if it has changed
                                        if ( oldState.name != newState.name ) {
                                            generate = true;
                                            break;
                                        }

                                        // compare tags if it has changed
                                        if ( oldState.tag != newState.tag )
                                        {
                                            generate = true;
                                            break;
                                        }
                                    }
                                }
                            }

                        if ( generate ) // break out of layers loop
                            break;
                        }                 
                    }

                if ( generate ) // break out of animators loop
                    break;                    
                }
            }
        }

        private string GetScriptName()
        {
            StringBuilder name = new StringBuilder( this.ToString() );
            int dotIndex = name.ToString().IndexOf('.');
            name.Remove( 0, dotIndex+1 );

            return name.ToString();
        }

        static List<ConstGenSettings.StatesCTRLR> RetriveValues()
        {
            // find controller GUIDs and create StatesCTRLR list
            string[] controllers = AssetDatabase.FindAssets("t:animatorcontroller");
            List<ConstGenSettings.StatesCTRLR> statesControllers = new List<ConstGenSettings.StatesCTRLR>();

            foreach (string CTRLR in controllers)  
            {
                // get controller and it's path
                string path = AssetDatabase.GUIDToAssetPath(CTRLR);
                UnityEditor.Animations.AnimatorController animCTRLR = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path);
                
                ConstGenSettings.StatesCTRLR stateCTRLR = new ConstGenSettings.StatesCTRLR();
                stateCTRLR.name = animCTRLR.name;

                if (animCTRLR.layers.Length == 0) continue;

                // loop throug layers
                for (int i = 0; i < animCTRLR.layers.Length; i++)
                {
                    var layer_ = animCTRLR.layers[i]; // get layer
      
                    ConstGenSettings.AnimLayer animLayer = new ConstGenSettings.AnimLayer();
                    animLayer.name = layer_.name; // store layer name

                    if ( layer_.stateMachine.states.Length == 0 )
                        continue;

                    // loop through layer states
                    for (int i2 = 0; i2 < layer_.stateMachine.states.Length; i2++)
                    {
                        var state_ = layer_.stateMachine.states[i2]; // get state
                        
                        ConstGenSettings.AnimState animState = new ConstGenSettings.AnimState();
                        animState.name = state_.state.name; // store state name
                        animState.tag = state_.state.tag; // store tag name

                        // add state to layers
                        animLayer.animStates.Add( animState ); 
                    }     

                    // add layers to controller
                    stateCTRLR.animLayers.Add( animLayer );      
                }

                // add controllers to list of controllers
                statesControllers.Add( stateCTRLR );
            }

            return statesControllers;
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
                        // loop through all animators
                        for (int i = 0; i < instance.newStateCTRLRS.Count; i++)
                        {
                            // cache current animator 
                            ConstGenSettings.StatesCTRLR animState = instance.newStateCTRLRS[i];

                            // write animator name header
                            using ( new CurlyBrackets(content, "public static class " + generator.MakeIdentifier(animState.name), indentCount) )
                            {
                                // loop through layers
                                for (int i2 = 0; i2 < animState.animLayers.Count; i2++)
                                {
                                    // cache current layer
                                    ConstGenSettings.AnimLayer animLayer = animState.animLayers[i2];

                                    // write layer group header name
                                    using ( new CurlyBrackets( content, "public static class " + generator.MakeIdentifier(animLayer.name), indentCount ) )
                                    {
                                        // loop through states
                                        for (int i3 = 0; i3 < animLayer.animStates.Count; i3++)
                                        {
                                            // cache current state
                                            ConstGenSettings.AnimState state_ = animLayer.animStates[i3];

                                            // write state name group at header
                                            using ( new CurlyBrackets( content, "public static class " + generator.MakeIdentifier(state_.name), indentCount ) )
                                            {
                                                // write state name
                                                content.WriteIndentedFormatLine(indentCount, 
                                                    "public const string name = @\"{0}\";", state_.name);

                                                // write state tag if present
                                                if ( state_.tag != string.Empty && state_.tag != null ) {
                                                    content.WriteIndentedFormatLine(indentCount, 
                                                        "public const string tag = @\"{0}\";", state_.tag);                                                    
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }                    
                }
            });
        }
    }
}






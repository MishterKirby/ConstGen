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
    public class AnimParamsGen
    {
        #region Variables =================================================================================================================
        private const string FileName = "_ANIMPARAMS";
        
        /// <summary>
        /// An instance of the generator class itself
        /// it's main purpose is to instantiate and cache a ConstantGenerator as it's property
        /// to be used for generating the code
        /// </summary>
        private static AnimParamsGen instance;
        private ConstantGenerator generator_ = new ConstantGenerator();
        private static ConstantGenerator generator { get { return instance.generator_; } }
        private static string FilePath { get { return string.Format(ConstantGenerator.FilePathFormat, generator.GetOutputPath(), FileName); } }
        private bool regenerateOnMissing;
        private bool updateOnReload;
        private List<ConstGenSettings.ParamsCTRLR> newParameters;
        private List<ConstGenSettings.ParamsCTRLR> oldParameters;
        #endregion Variables ===============================================================================================================

        // static constructor
        // get's called with the class when [InitializeOnLoad] happens
        static AnimParamsGen()
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
            instance = new AnimParamsGen();
            instance.generator_ = new ConstantGenerator();
        }

        private static void RetrieveSettingsData()
        {
            ConstGenSettings cgs = ConstantGenerator.GetSettingsFile();

            instance.regenerateOnMissing = cgs.regenerateOnMissing; 
            instance.updateOnReload = cgs.updateOnReload;
            instance.oldParameters = cgs._ANIMPARAMS;
        }

        /// <summary>
        /// Generates the file by writing new updated contents or generates the file is none is present
        /// </summary>
        public static void Generate()
        {
            CreateGeneratorInsance();
            instance.newParameters = RetriveValues();

            // store the new properties to SO
            ConstantGenerator.GetSettingsFile()._ANIMPARAMS.Clear();
            ConstantGenerator.GetSettingsFile()._ANIMPARAMS = instance.newParameters;

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
            instance.newParameters = RetriveValues();

            // check if the number of animation controllers in the assets has changed
            if ( instance.oldParameters.Count != instance.newParameters.Count ) {
                generate = true;
            } 
            else // else check for changes in the parameters of the controllers
            { 
                // loop through animators
                for (int i = 0; i < instance.oldParameters.Count; i++) 
                {
                    ConstGenSettings.ParamsCTRLR oldCTRLR = instance.oldParameters[i];
                    ConstGenSettings.ParamsCTRLR newCTRLR = instance.newParameters[i];

                    // check if the name of the controller has changed or
                    // if any parameters is added or removed
                    if ( oldCTRLR.name != newCTRLR.name || 
                        oldCTRLR.parameters.Count != newCTRLR.parameters.Count ) {

                        generate = true;
                        break;
                    }
                    else // else check if any of the name of paramters has changed
                    {
                        // loop through parameters
                        for (int i2 = 0; i2 < oldCTRLR.parameters.Count; i2++)
                        {
                            string oldName = oldCTRLR.parameters[i2];
                            string newName = newCTRLR.parameters[i2];

                            // compare parameter names
                            if ( oldName != newName ) 
                            {
                                generate = true;
                                break;
                            }
                        }
                    }

                    if ( generate ) // break out of animators loop
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

        static List<ConstGenSettings.ParamsCTRLR> RetriveValues()
        {
            // find controller GUIDs and create LayersCTRLR list
            string[] controllers = AssetDatabase.FindAssets("t:animatorcontroller");
            List<ConstGenSettings.ParamsCTRLR> animCTRLRS = new List<ConstGenSettings.ParamsCTRLR>();

            foreach (string CTRLR in controllers)
            {
                // get controller and it's path
                string path = AssetDatabase.GUIDToAssetPath(CTRLR);
                UnityEditor.Animations.AnimatorController item = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path);
                
                if (item.parameters.Length == 0) continue;

                ConstGenSettings.ParamsCTRLR controller = new ConstGenSettings.ParamsCTRLR();
                controller.name = item.name;

                // loop through controller's parameters and cache it
                foreach (var parameter in item.parameters)
                {
                    controller.parameters.Add( parameter.name );
                }

                animCTRLRS.Add( controller );
            }

            return animCTRLRS;
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
                        foreach (ConstGenSettings.ParamsCTRLR ctrlr in instance.newParameters)
                        {
                            // write animator owner header group of the parameters
                            content.WriteIndentedFormatLine(indentCount, "public static class {0}", generator.MakeIdentifier(ctrlr.name));
                            using (new CurlyBrackets(content, indentCount))
                            {
                                // write parameters
                                foreach (string parameters in ctrlr.parameters )
                                {
                                    content.WriteIndentedFormatLine(indentCount, 
                                        "public const string {0} = \"{1}\";", 
                                            generator.MakeIdentifier(parameters), 
                                                generator.EscapeDoubleQuote(parameters));
                                }
                            }
                            content.Append(Environment.NewLine);
                        }
                    }                    
                }
            });
        }
    }
}






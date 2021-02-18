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
    public class TagsGen 
    {
        #region Variables =================================================================================================================
        private const string FileName = "_TAGS";
        
        /// <summary>
        /// An instance of the generator class itself
        /// it's main purpose is to instantiate and cache a ConstantGenerator as it's property
        /// to be used for generating the code
        /// </summary>
        private static TagsGen instance;
        private ConstantGenerator generator_ = new ConstantGenerator();
        private static ConstantGenerator generator { get { return instance.generator_; } }
        private static string FilePath { get { return string.Format(ConstantGenerator.FilePathFormat, generator.GetOutputPath(), FileName); } }
        
        private bool regenerateOnMissing;
        private bool updateOnReload;
        private List<string> newTags;
        private List<string> oldTags;
        #endregion Variables ===============================================================================================================

        // static constructor
        // get's called with the class when [InitializeOnLoad] happens
        static TagsGen()
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
                if ( instance.updateOnReload )
                    UpdateFile();
            }
        }

        private static void CreateGeneratorInsance()
        {
            if (instance != null) return;

            // create self instance of the class
            // then create and cache the ConstantGenerator instance to it's property
            instance = new TagsGen();
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
                instance.oldTags = cgs._TAGS;

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
            instance.newTags = RetriveValues();

            // store the new properties to SO
            ConstantGenerator.GetSettingsFile()._TAGS.Clear();
            ConstantGenerator.GetSettingsFile()._TAGS = instance.newTags;

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

            instance.newTags = RetriveValues();
            List<string> differences = instance.newTags.Except( instance.oldTags ).ToList();

            if (differences.Count > 0 || instance.newTags.Count != instance.oldTags.Count)
                Generate(); 
        }

        private string GetScriptName()
        {
            StringBuilder name = new StringBuilder( this.ToString() );
            int dotIndex = name.ToString().IndexOf('.');
            name.Remove( 0, dotIndex+1 );

            return name.ToString();
        }


        static List<string> RetriveValues()
        {
            return InternalEditorUtility.tags.ToList();
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
                        foreach (string property in instance.newTags)
                        {
                            content.WriteIndentedFormatLine(indentCount, "public const string {0} = @\"{1}\";", 
                                generator.MakeIdentifier(property), generator.EscapeDoubleQuote(property));
                        }
                    }                    
                }
            });
        }
    }
}






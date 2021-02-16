using UnityEngine;
using UnityEditor;

// use alias directives so we can access easier through shorther names
using EGL = UnityEditor.EditorGUILayout;
using GL = UnityEngine.GUILayout;

namespace ConstGen
{

    public class ConstGenWindow : EditorWindow {

        private static ConstGenSettings settings;
        private static ConstGenWindow window;
        private string generatorName;
        private string outputFileName;
        private bool showFoldOut = true;
        private Texture logo;
        private Texture border;

        [MenuItem("Const Gen/Generator")]
        private static void ShowWindow() {
            window = GetWindow<ConstGenWindow>();
            window.titleContent = new GUIContent("Const Generator");
            window.minSize = new Vector2( 300, 398 );
            // window.maxSize = new Vector2( 300, 400 );
            window.Show();
        }
        private void OnGUI() {

            if ( settings == null ) {
                settings = ConstantGenerator.GetSettingsFile();
            }

            if ( logo == null ) {
                logo = ConstantGenerator.GetLogo();
            }

            if ( border == null ) {
                border = ConstantGenerator.GetBorder();
            }

            EditorGUI.BeginChangeCheck();

            StartGUI( "Layers" );
                if ( DrawGenButton() )
                {
                    LayersGen.Generate();
                    window.Close();
                }

                if ( DrawForceGenButton() )
                {
                    LayersGen.ForceGenerate();           
                    window.Close();   
                }
            EndGUI();
            // -------------------------------------------------------------------------------------
            StartGUI( "Tags" );
                if ( DrawGenButton() )
                {
                    TagsGen.Generate();
                    window.Close();
                }

                if ( DrawForceGenButton() )
                {
                    TagsGen.ForceGenerate();
                    window.Close();   
                }               
            EndGUI();
            // -------------------------------------------------------------------------------------
            StartGUI( "Sort Layers" );
                if ( DrawGenButton() )
                {
                    SortingLayersGen.Generate();
                    window.Close();
                }

                if ( DrawForceGenButton() )
                {
                    SortingLayersGen.ForceGenerate();
                    window.Close();   
                }                
            EndGUI();
            // -------------------------------------------------------------------------------------
            StartGUI( "Scenes" );
                if ( DrawGenButton() )
                {
                    ScenesGen.Generate();
                    window.Close();
                }

                if ( DrawForceGenButton() )
                {
                    ScenesGen.ForceGenerate();
                    window.Close();   
                }               
            EndGUI();
            // -------------------------------------------------------------------------------------
            StartGUI( "Shader Props" );
                if ( DrawGenButton() )
                {
                    ShaderPropsGen.Generate(false);
                    window.Close();
                }

                if ( DrawForceGenButton() )
                {
                    ShaderPropsGen.ForceGenerate();
                    window.Close();   
                }               
            EndGUI();
            // -------------------------------------------------------------------------------------
            StartGUI( "Anim Params" );
                if ( DrawGenButton() )
                {
                    AnimParamsGen.Generate();
                    window.Close();
                }

                if ( DrawForceGenButton() )
                {
                    AnimParamsGen.ForceGenerate();
                    window.Close();   
                }               
            EndGUI();
            // -------------------------------------------------------------------------------------
            StartGUI( "Anim Layers" );
                if ( DrawGenButton() )
                {
                    AnimLayersGen.Generate();
                    window.Close();
                }

                if ( DrawForceGenButton() )
                {
                    AnimLayersGen.ForceGenerate();
                    window.Close();   
                }               
            EndGUI();
            // -------------------------------------------------------------------------------------
            StartGUI( "Anim States" );
                if ( DrawGenButton() )
                {
                    AnimStatesGen.Generate();
                    window.Close();
                }

                if ( DrawForceGenButton() )
                {
                    AnimStatesGen.ForceGenerate();
                    window.Close();   
                }                
            EndGUI();
            // =========================================================================================
            DrawLine( Color.white, 2, 5 );

            GUIStyle style = new GUIStyle( GUI.skin.button );
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;

            EGL.BeginHorizontal();
                EGL.BeginVertical();
                        EGL.BeginHorizontal();
                        if ( GL.Button( "GENERATE ALL", style ) )
                        {
                            LayersGen.Generate();
                            TagsGen.Generate();
                            SortingLayersGen.Generate();
                            ScenesGen.Generate();
                            ShaderPropsGen.Generate( false );
                            AnimParamsGen.Generate();
                            AnimLayersGen.Generate();
                            AnimStatesGen.Generate();
                            window.Close();
                        } 
                        GL.FlexibleSpace();
                        EGL.EndHorizontal();

                        EGL.BeginHorizontal();
                        if ( GL.Button( "FORCE GENERATE ALL", style ) )
                        {
                            LayersGen.ForceGenerate();
                            TagsGen.ForceGenerate();
                            SortingLayersGen.ForceGenerate();
                            ScenesGen.ForceGenerate();
                            ShaderPropsGen.ForceGenerate();
                            AnimParamsGen.ForceGenerate();
                            AnimLayersGen.ForceGenerate();
                            AnimStatesGen.ForceGenerate();
                            window.Close();
                        } 
                        GL.FlexibleSpace();
                        EGL.EndHorizontal();
                EGL.EndVertical();
                EGL.BeginVertical();
            // ---------------------------------------------------------------------------------------
                        Color genOnReloadColor;
                        Color updateOnReloadColor;

                        if ( settings.regenerateOnMissing ) {
                            genOnReloadColor =  Color.green * 2;
                        } else {
                            genOnReloadColor =  Color.white * 1.5f;
                        }

                        if ( settings.updateOnReload ) {
                            updateOnReloadColor =  Color.green * 2;
                        } else {
                            updateOnReloadColor =  Color.white * 1.5f;
                        }
                                        
                        EGL.BeginHorizontal();
                            GUI.backgroundColor = genOnReloadColor;
                            if ( GL.Button( new GUIContent("ReGen On Missing", "Automatically re-generates the constants file is none is present."), style ) ) {
                                settings.regenerateOnMissing = !settings.regenerateOnMissing;
                                EditorUtility.SetDirty( settings );
                            } 
                        EGL.EndHorizontal();

                        EGL.BeginHorizontal();
                            GUI.backgroundColor = updateOnReloadColor;
                            if ( GL.Button( new GUIContent("Update On Reload", "Automatically re-generates the constants on editor recompile if any changes are detected."), style) ) {
                                settings.updateOnReload = !settings.updateOnReload;
                                EditorUtility.SetDirty( settings );
                            } 
                        EGL.EndHorizontal();

                EGL.EndVertical();
            EGL.EndHorizontal();
            // =========================================================================================
            DrawLine( Color.white, 2, 5 );

            GUI.backgroundColor = Color.gray;
            GUI.contentColor = Color.white * 10;
            showFoldOut = EGL.BeginFoldoutHeaderGroup( showFoldOut, "Generate Generator Script" );

                if ( showFoldOut )
                {
                    GL.Space(5);
                    GUI.contentColor = Color.white * 10;
                    generatorName = EGL.TextField( "Generator Name" ,generatorName );
                    outputFileName = EGL.TextField( "Output File Name" ,outputFileName );

                    GL.Space(5);
                    EGL.BeginHorizontal();

                    if ( !settings.regenerateOnMissing )
                    {
                        EGL.BeginVertical();
                        GL.FlexibleSpace();
                        EGL.HelpBox("NOTE: Force Generate will only delete the file but will NOT generate a new one if the [ReGen On Missing] is turned off", 
                            MessageType.Warning);
                        EGL.EndVertical();
                    } 
                    else 
                    {   // ============================================================================
                        // Draw Ma Awesome Logo
                        EGL.BeginVertical();
                            GL.FlexibleSpace();
                            Rect horiRect = EGL.BeginHorizontal();
                            
                                Rect boxRect = new Rect( horiRect.x+3, horiRect.y-54, 125, 52 );

                                Rect backgroundRect = boxRect;
                                backgroundRect.width = border.width;
                                backgroundRect.height = border.height;
                                GUI.DrawTexture( backgroundRect, border );
                                // GUI.Box( boxRect, iconBackground, );

                                GUI.Label( new Rect( boxRect.x+3, boxRect.y+16, 100, 20 ), "Created BY: " );

                                Rect logoRect = new Rect( boxRect.x+76, boxRect.y+2, logo.width, logo.height );
                                GUI.DrawTexture( logoRect, logo);

                            EGL.EndHorizontal();
                        EGL.EndVertical();
                        // ============================================================================
                    }

                    GL.FlexibleSpace();

                    GUI.contentColor = Color.white * 5;
                        EGL.BeginVertical();
                            GL.FlexibleSpace();
                            GUI.backgroundColor = Color.white * 2.5f;
                            GUI.contentColor = Color.black * 5;
                            if ( GL.Button( "Create", new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold, fontSize = 12 }, 
                                GL.Width(75), GL.Height(30) ) )
                            {
                                if ( generatorName == string.Empty || outputFileName == string.Empty  )
                                {
                                    Debug.LogWarning( "Fill out all the fields" );
                                } 
                                else
                                {
                                    TemplateGen.GenerateTemplate( generatorName, outputFileName );
                                    window.Close();
                                }
                            }
                        EGL.EndVertical();
                    GL.Space(1);
                    EGL.EndHorizontal();
                }
            EGL.EndFoldoutHeaderGroup();

            if ( EditorGUI.EndChangeCheck() )
            {
                EditorUtility.SetDirty( settings );
            }
        }

        private void StartGUI( string name )
        {
            Rect r =  EGL.BeginVertical( );
            GL.Space(3);
            EGL.BeginHorizontal();
                GUI.backgroundColor = Color.black * 2;
                GUI.contentColor = Color.white;
                GUI.Box( r, GUIContent.none );
                GUI.contentColor = Color.white * 10;
                EGL.LabelField(" " + name, EditorStyles.boldLabel, GL.Width(85));            
        }

        private bool DrawGenButton()
        {
            GUI.backgroundColor = Color.white * 2.5f;
            GUI.contentColor = Color.black * 5;
            bool clicked = GL.Button( 
                new GUIContent("Generate","Generates the file by writing new updated contents or generates the file is none is present."), 
                GL.Height(20) 
            );
            return clicked;
        }

        private bool DrawForceGenButton()
        {
            bool clicked = GL.Button( 
                new GUIContent("Force Generate", "Deletes the current file thus forcing the generator to create a new one."), 
                GL.Height(20) 
            );
            return clicked;
        }    

        private void EndGUI()
        {
                GL.Space(5);
            EGL.EndHorizontal();    
            GL.Space(2);
            EGL.EndVertical();
        }   

        private void DrawLine( Color color, int thickness = 2, int padding = 10 )
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding+thickness));
            r.height = thickness;
            r.y+=padding/2;
            r.x-=2;
            r.width +=6;
            EditorGUI.DrawRect(r, color);        
        } 
    }   
}
using Naninovel;
using System;
using UnityEditor;
using UnityEngine;

namespace AmoyFeels.SpriteShaderIntegration
{
    // Create the custom editor, that will used when drawing the affected fields.
    // The script should be inside an `Editor` folder, as it uses `UnityEditor` API.
    [CustomPropertyDrawer(typeof(MaterialEditorSSIAttribute))]
    public class MaterialPropertyDrawer : PropertyDrawer
    {
        private const string SHADER_ID = "ShaderInstanceID";

        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.PropertyField(rect, prop);
            var propertyCopy = prop.Copy();
            FindSampleMaterialField(propertyCopy);


        }

        void FindSampleMaterialField(SerializedProperty serializeSampleMaterial)
        {
            if (serializeSampleMaterial == null || serializeSampleMaterial is null)
                return;
            var material = serializeSampleMaterial.objectReferenceValue as Material;

            var properties = serializeSampleMaterial.Copy();
            properties.Next(false);

            var types = properties.Copy();
            types.Next(false);

            bool isFilled = false;

            // If material null then clear the Properties and Types of shader
            bool isMaterialNull = !ObjectUtils.IsValid(material);
            if (isMaterialNull)
            {
                isFilled = ClearAll();
            }
            else
                isFilled = true;

            // Check if the material has same shader name. If not same, then clear the Properties and Types of shader
            if (!isMaterialNull)
            {
                int shaderID = EditorPrefs.GetInt(SHADER_ID, material.shader.GetInstanceID());
                if (shaderID != material.shader.GetInstanceID())
                {
                    isFilled = ClearAll();
                }
                else
                    isFilled = true;
            }

            // Check if the Properties and Types is still filled, if it true, then stop here, no need to create array again.
            if (isFilled)
                return;
            if (material == null || material is null)
                isFilled = ClearAll();

            ClearAll();
            var totalProperties = material == null || material is null ? 0 : ShaderUtil.GetPropertyCount(material.shader);
            if (totalProperties <= 0)
            {
                ClearAll();
                return;
            }
            var totalProps = 0;
            for (int i = 0; i < totalProperties; ++i)
            {
                EditorUtility.DisplayProgressBar("Loading Material...", "Searching valid shader properties and types...", i / (float)totalProperties);
                var type = (int)ShaderUtil.GetPropertyType(material.shader, i);
                bool isFloat = (CharacterMetadataSSI.PropertyType)type == CharacterMetadataSSI.PropertyType.Float;
                bool isRange = (CharacterMetadataSSI.PropertyType)type == CharacterMetadataSSI.PropertyType.Range;
                bool isColor = (CharacterMetadataSSI.PropertyType)type == CharacterMetadataSSI.PropertyType.Color;

                if (!(isFloat || isRange || isColor
#if UNITY_2021_1_OR_NEWER
                    || (CharacterMetadataSSI.PropertyType)type == CharacterMetadataSSI.PropertyType.Int
#endif
                    ))
                    continue;
                totalProps++;
                types.InsertArrayElementAtIndex(types.arraySize);
                types.GetArrayElementAtIndex(types.arraySize - 1).enumValueIndex = type;

                var name = ShaderUtil.GetPropertyName(material.shader, i);
                properties.InsertArrayElementAtIndex(properties.arraySize);
                properties.GetArrayElementAtIndex(properties.arraySize - 1).stringValue = name;

            }
            EditorUtility.ClearProgressBar();

            Debug.Log("Assignee shader of (" + material.shader.name + ") with total " + totalProps + " of properties");

            isFilled = true;
            serializeSampleMaterial.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            properties.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            types.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorPrefs.SetInt(SHADER_ID, material.shader.GetInstanceID());
            return;

            bool ClearAll()
            {
                properties.ClearArray();
                types.ClearArray();
                return false;
            }
        }
    }

}
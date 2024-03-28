using Naninovel;
using System;
using UnityEditor;
using UnityEngine;

namespace AmoyFeels.SpriteShaderIntegration
{
    public class CharacterMetadataEditorSSI : CharacterMetadataEditor
    {
        private const string CUSTOM_DATA = "customData";
        private bool isFilled;
        private bool isValid;
        private const string SHADER_ID = "ShaderInstanceID";
        protected override Action<SerializedProperty> GetCustomDrawer(string propertyName)
        {
            return base.GetCustomDrawer(propertyName);
        }

        public void DrawMaterialField(SerializedProperty serializedProperty, ActorMetadata metadata)
        {
            var propertyCopy = serializedProperty.Copy();
            var endProperty = propertyCopy.GetEndProperty();

            propertyCopy.NextVisible(true);
            do
            {
                if (SerializedProperty.EqualContents(propertyCopy, endProperty))
                    break;
                ImplementationValidator(propertyCopy);
                if (FindSampleMaterialField(propertyCopy))
                    continue;
            } while (propertyCopy.NextVisible(false));


        }

        private void ImplementationValidator(SerializedProperty propertyCopy)
        {
            if (!propertyCopy.propertyPath.EndsWithFast(nameof(ActorMetadata.Implementation)))
                return;
            isValid = propertyCopy.stringValue.EqualsFast(typeof(SpriteCharacterSSI).AssemblyQualifiedName);
        }

        bool FindSampleMaterialField(SerializedProperty serializedProperty)
        {
            if (!isValid)
                return false;

            if (!serializedProperty.propertyPath.EndsWithFast(CUSTOM_DATA))
                return false;
            SerializedProperty serializeSampleMaterial = serializedProperty.FindPropertyRelative(nameof(CharacterMetadataSSI.SampleMaterial));
            if (serializeSampleMaterial == null || serializeSampleMaterial is null)
                return false;
            var material = serializeSampleMaterial.objectReferenceValue as Material;


            SerializedProperty properties = serializedProperty.FindPropertyRelative(nameof(CharacterMetadataSSI.Properties));
            SerializedProperty types = serializedProperty.FindPropertyRelative(nameof(CharacterMetadataSSI.Types));

            // If material null then clear the Properties and Types of shader
            bool isMaterialNull = material == null || material is null;
            if (isMaterialNull)
                isFilled = ClearAll();

            // Check if the material has same shader name. If not same, then clear the Properties and Types of shader
            if (!isMaterialNull)
            {
                int shaderID = EditorPrefs.GetInt(SHADER_ID, material.shader.GetInstanceID());
                if (shaderID != material.shader.GetInstanceID())
                {
                    isFilled = ClearAll();
                }
            }

            // Check if the Properties and Types is still filled, if it true, then stop here, no need to create array again.
            if (isFilled)
                return false;
            if (material == null || material is null)
                isFilled = ClearAll();

            ClearAll();
            var totalProperties = material == null || material is null ? 0 : ShaderUtil.GetPropertyCount(material.shader);
            if (totalProperties <= 0)
            {
                return ClearAll();
            }
            for (int i = 0; i < totalProperties; ++i)
            {
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
                types.InsertArrayElementAtIndex(types.arraySize);
                types.GetArrayElementAtIndex(types.arraySize - 1).enumValueIndex = type;

                var name = ShaderUtil.GetPropertyName(material.shader, i);
                properties.InsertArrayElementAtIndex(properties.arraySize);
                properties.GetArrayElementAtIndex(properties.arraySize - 1).stringValue = name;

            }

            isFilled = true;
            serializedProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorPrefs.SetInt(SHADER_ID, material.shader.GetInstanceID());
            return true;

            bool ClearAll()
            {
                properties.ClearArray();
                types.ClearArray();
                return false;
            }
        }

    }

    [OverrideSettings]
    public class CharacterSettingsSSI : CharactersSettings
    {
        protected override MetadataEditor<ICharacterActor, CharacterMetadata> MetadataEditor { get; } = new CharacterMetadataEditorSSI();

        protected override void DrawMetaEditor(SerializedProperty serializedProperty)
        {
            (MetadataEditor as CharacterMetadataEditorSSI).DrawMaterialField(serializedProperty, EditedMetadata);
            base.DrawMetaEditor(serializedProperty);
        }

    }

}
using Naninovel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AmoyFeels.SpriteShaderIntegration
{
    public class CharacterMetadataSSI : CustomMetadata<SpriteCharacterSSI>
    {
        public Material SampleMaterial;
        [HideInInspector] public List<string> Properties = new List<string>();
        [HideInInspector] public List<PropertyType> Types = new List<PropertyType>();

        public IDictionary<string, float> GetPropertyFloats()
        {
            var propertyFloats = Properties
                .Zip(Types, (k, v) => new { Key = k, Value = v })
                .Where(x => x.Value == PropertyType.Float || x.Value == PropertyType.Range)
                .ToDictionary(x => x.Key, x => SampleMaterial.GetFloat(x.Key));

            return propertyFloats;
        }

        public IDictionary<string, Color> GetPropertyColors()
        {
            var propertyColors = Properties
                .Zip(Types, (k, v) => new { Key = k, Value = v })
                .Where(x => x.Value == PropertyType.Color)
                .ToDictionary(x => x.Key, x => SampleMaterial.GetColor(x.Key));

            return propertyColors;
        }

        public IDictionary<string, int> GetPropertyInts()
        {
            var propertyInts = Properties
                .Zip(Types, (k, v) => new { Key = k, Value = v })
                .Where(x => x.Value == PropertyType.Int)
#if UNITY_2022_1_OR_NEWER
                .ToDictionary(x => x.Key, x => SampleMaterial.GetInteger(x.Key));
#else
                .ToDictionary(x => x.Key, x => SampleMaterial.GetInt(x.Key));
#endif
            return propertyInts;
        }

        public enum PropertyType
        {
            //
            // Summary:
            //     Color Property.
            Color,
            //
            // Summary:
            //     Vector Property.
            Vector,
            //
            // Summary:
            //     Float Property.
            Float,
            //
            // Summary:
            //     Range Property.
            Range,
            //
            // Summary:
            //     Texture Property.
            TexEnv,
            //
            // Summary:
            //     Int Property.
            Int
        }
    }

}
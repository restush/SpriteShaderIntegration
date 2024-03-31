using Naninovel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AmoyFeels.SpriteShaderIntegration.SSIManager.GameState;

namespace AmoyFeels.SpriteShaderIntegration
{
    [InitializeAtRuntime(10)]
    public class SSIManager : IStatefulService<GameStateMap>
    {
        private readonly ICharacterManager characterManager;
        private readonly IStateManager stateManager;

        public CharactersConfiguration CharactersConfiguration { get; }

        public SSIManager(ICharacterManager characterManager, IStateManager stateManager, CharactersConfiguration charactersConfiguration)
        {
            this.characterManager = characterManager;
            this.stateManager = stateManager;
            CharactersConfiguration = charactersConfiguration;

        }

        public T GetDefaultValueProperty<T>(string actorID, string propertyID) where T : struct
        {
            var metadata = CharactersConfiguration.GetMetadataOrDefault(actorID).GetCustomData<CharacterMetadataSSI>();
            if (typeof(T) == typeof(float))
            {
                return (T)(object)metadata.SampleMaterial.GetFloat(propertyID);
            }
            else if (typeof(T) == typeof(Color))
            {
                return (T)(object)metadata.SampleMaterial.GetColor(propertyID);
            }
            else return default;
        }

        [System.Serializable]
        public class GameState
        {
            // cache
            public CharacterShaderState[] characterShaderStates;

            [System.Serializable]
            public class CharacterShaderState
            {
                public string characterID;
                public FloatState[] floatState;
                public IntState[] intState;
                public ColorState[] colorState;
            }

            [System.Serializable]
            public class FloatState : EffectState<float>
            {

            }

            [System.Serializable]
            public class IntState : EffectState<int>
            {

            }
            [System.Serializable]
            public class ColorState : EffectState<Color>
            {

            }
            [System.Serializable]
            public abstract class EffectState
            {
                public string PropertyID;

                public abstract object GetValue();
                public abstract void SetValue(object value);
                public void Set(string propertyID)
                {
                    PropertyID = propertyID;
                }
            }

            [System.Serializable]
            public abstract class EffectState<T> : EffectState where T : struct
            {
                public T Value;

                public EffectState() { }

                public void Set(string propertyID, T value = default)
                {
                    PropertyID = propertyID;
                    Value = value;
                }

                public override object GetValue() => Value;
                public override void SetValue(object value) => Value = (T)value;
                public override string ToString()
                {
                    return "Property ID: " + PropertyID + "| Value: " + Value.ToString();
                }
            }

        }

        public void DestroyService()
        {

        }

        public UniTask InitializeServiceAsync()
        {
            return UniTask.CompletedTask;
        }

        public UniTask LoadServiceStateAsync(GameStateMap state)
        {
            var saveData = state.GetState<GameState>();
            if (saveData == null)
                return UniTask.CompletedTask; // do nothing.

            if (stateManager.RollbackInProgress)
                stateManager.OnRollbackFinished += LoadState;
            else
                LoadState();


            return UniTask.CompletedTask;

            void LoadState()
            {
                stateManager.OnRollbackFinished -= LoadState;
                foreach (ICharacterActor character_InTheScene in characterManager.GetAllActors())
                {
                    if (!(character_InTheScene is SpriteCharacterSSI spriteCharacterSSI))
                        continue;
                    var transitionalSprite = spriteCharacterSSI.GetTransitionalSpriteSSI();
                    if (transitionalSprite == null)
                        continue;

                    foreach (var characterShaderState in saveData.characterShaderStates)
                    {
                        foreach (var colorState in characterShaderState.colorState)
                            transitionalSprite.SetColor(colorState.PropertyID, colorState.Value);

                        foreach (var floatState in characterShaderState.floatState)
                            transitionalSprite.SetFloat(floatState.PropertyID, floatState.Value);
                    }

                }

            }
        }

        public void ResetService()
        {

        }

        public void SaveServiceState(GameStateMap state)
        {
            Save();

            void Save(GameSaveLoadArgs arg = default)
            {
                stateManager.OnGameSaveFinished -= Save;
                var data = new GameState();
                List<CharacterShaderState> characterShaderStates = new List<CharacterShaderState>();

                var actors = characterManager.GetAllActors().Where(x => x is SpriteCharacterSSI ss && ss != null).Select(x => x as SpriteCharacterSSI);
                foreach (var actor in actors)
                {
                    var transitionalSprite = actor.GetTransitionalSpriteSSI();
                    var meta = this.CharactersConfiguration.GetMetadataOrDefault(actor.Id).GetCustomData<CharacterMetadataSSI>();

                    var propColors = meta.GetPropertyColors()
                        .Where(x => !transitionalSprite.GetColor(x.Key).Equals(x.Value))
                        .Select(x => new ColorState() { PropertyID = x.Key, Value = transitionalSprite.GetColor(x.Key) })
                        .ToArray();

                    var propFloats = meta.GetPropertyFloats()
                        .Where(x => transitionalSprite.GetFloat(x.Key) != x.Value)
                        .Select(x => new FloatState() { PropertyID = x.Key, Value = transitionalSprite.GetFloat(x.Key) }).
                        ToArray();

                    var propInts = meta.GetPropertyInts()
                        .Where(x => transitionalSprite.GetInt(x.Key) != x.Value)
                        .Select(x => new IntState() { PropertyID = x.Key, Value = transitionalSprite.GetInt(x.Key) })
                        .ToArray();

                    var charaState = new CharacterShaderState()
                    {
                        characterID = actor.Id,
                        colorState = propColors,
                        floatState = propFloats,
                        intState = propInts,
                    };
                    characterShaderStates.Add(charaState);
                }

                data.characterShaderStates = characterShaderStates.ToArray();
                state.SetState(data);
            }
        }
    }

}
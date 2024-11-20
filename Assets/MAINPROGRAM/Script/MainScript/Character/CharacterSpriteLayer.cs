using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
    public class CharacterSpriteLayer
    {
        private CharacterManager characterManager => CharacterManager.Instance;

        private const float Default_Transition_Speed = 2f;
        private float transitionSpeedMultiplier = 1f;

        public int layer { get; private set; } = 0;
        public Image renderer { get; private set; } = null;
        public CanvasGroup rendererCG => renderer.GetComponent<CanvasGroup>();

        private List<CanvasGroup> oldRenderer = new List<CanvasGroup>();

        private Coroutine co_transitioningLayer = null;
        private Coroutine co_LevelingAlpha = null;
        private Coroutine co_ChangingColor = null;
        private Coroutine co_Flipping = null;
        private bool isFacingLeft = Character.Default_Orientation_Is_Facing_left;

        public bool isTransitioningLayer => co_transitioningLayer != null;
        public bool isLevelingAlpha => co_LevelingAlpha != null;
        public bool isChangingColor => co_ChangingColor != null;
        public bool isFlipping => co_Flipping != null;

        public CharacterSpriteLayer(Image defaultRenderer, int layer = 0)
        {
            renderer = defaultRenderer;
            this.layer = layer;
        }

        public void SetSprite(Sprite sprite)
        {
            if (renderer != null)
            {
                renderer.sprite = sprite;
            }
            else
                Debug.LogWarning("Missing renderer");
        }

        public Coroutine TransitionSprite(Sprite sprite, float speed = 1)
        {
            if (sprite == renderer.sprite)
                return null;

            if (isTransitioningLayer)
                characterManager.StopCoroutine(co_transitioningLayer);

            co_transitioningLayer = characterManager.StartCoroutine(TransitioningSprite(sprite, speed));

            return co_transitioningLayer;
        }

        private IEnumerator TransitioningSprite(Sprite sprite, float speedMultiplier)
        {
            Image newRenderer = CreateRenderer(renderer.transform.parent);
            newRenderer.sprite = sprite;

            yield return TryStartLevelingAlpha();

            co_transitioningLayer = null;
        }

        private Image CreateRenderer(Transform parent)
        {
            Image newRenderer = Object.Instantiate(renderer, parent);
            oldRenderer.Add(rendererCG);

            newRenderer.name = renderer.name;
            renderer = newRenderer;
            rendererCG.alpha = 0; // Ensure this is set to 0 for fading in

            return newRenderer;
        }

        private Coroutine TryStartLevelingAlpha()
        {
            if (isLevelingAlpha)
            {
                // Optionally log that we're trying to start the alpha leveling
                Debug.Log("Alpha leveling is already in progress.");
                return co_LevelingAlpha; // Return existing coroutine if it's already running
            }

            co_LevelingAlpha = characterManager.StartCoroutine(RunAlphaLeveling());
            return co_LevelingAlpha;
        }

        private IEnumerator RunAlphaLeveling()
        {
            // Ensure new renderer starts with alpha 0
            rendererCG.alpha = 0;
            float speed = Default_Transition_Speed * transitionSpeedMultiplier;

            while (rendererCG.alpha < 1 || oldRenderer.Any(oldCG => oldCG.alpha > 0))
            {
                // Adjust alpha of the new renderer for fade-in effect
                rendererCG.alpha = Mathf.MoveTowards(rendererCG.alpha, 1, speed * Time.deltaTime);
                Debug.Log($"New Renderer Alpha: {rendererCG.alpha}");

                // Adjust alpha of old renderers for fade-out effect
                for (int i = oldRenderer.Count - 1; i >= 0; i--)
                {
                    CanvasGroup oldCG = oldRenderer[i];
                    oldCG.alpha = Mathf.MoveTowards(oldCG.alpha, 0, speed * Time.deltaTime);
                    Debug.Log($"Old Renderer {i} Alpha: {oldCG.alpha}");

                    if (oldCG.alpha <= 0)
                    {
                        oldRenderer.RemoveAt(i);
                        Object.Destroy(oldCG.gameObject);
                    }
                }

                yield return null; // Wait for the next frame
            }

            co_LevelingAlpha = null; // Reset coroutine reference after completion
        }

        public void SetColor(Color color)
        {
            renderer.color = color;

            foreach(CanvasGroup oldCG in oldRenderer)
            {
                oldCG.GetComponent<Image>().color = color;
            }
        }

        public Coroutine TransisitioColor(Color color, float speed = 1f)
        {
            if (isChangingColor)
                characterManager.StopCoroutine(co_ChangingColor);

            co_ChangingColor = characterManager.StartCoroutine(ChangingColor(color, speed));

            return co_ChangingColor;
        }

        public void StopChangingColor()
        {
            if (!isChangingColor)
                return;

            characterManager.StopCoroutine(co_ChangingColor);
            co_ChangingColor = null;
        }

        private IEnumerator ChangingColor(Color color, float speedMultiplier)
        {
            Color oldColor = renderer.color;
            List<Image> oldImages = new List<Image>();

            foreach (var oldCG in oldRenderer)
            {
                oldImages.Add(oldCG.GetComponent<Image>());
            }

            float colorPercent = 0;
            while (colorPercent < 1)
            {
                colorPercent += Default_Transition_Speed * speedMultiplier * Time.deltaTime;
                renderer.color = Color.Lerp(oldColor, color, colorPercent);

                foreach(Image oldImage in oldImages)
                {
                    oldImage.color = renderer.color;
                }

                yield return null;
            }
            co_ChangingColor = null;
        }

        public Coroutine Flip(float speed = 1, bool immediate = false)
        {
            if (isFacingLeft)
                return FaceRight(speed , immediate);
            else
                return FaceLeft(speed, immediate);
        }

        public Coroutine FaceLeft(float speed, bool immediate = false)
        {
            if (isFlipping)
                characterManager.StopCoroutine(co_Flipping);

            isFacingLeft = true;
            co_Flipping = characterManager.StartCoroutine(FaceDirection(isFacingLeft, speed, immediate));
            return co_Flipping;
        }

        public Coroutine FaceRight(float speed, bool immediate = false)
        {
            if (isFlipping)
                characterManager.StopCoroutine(co_Flipping);

            isFacingLeft = false;
            co_Flipping = characterManager.StartCoroutine(FaceDirection(isFacingLeft, speed,immediate));
            return co_Flipping;
        }
        private IEnumerator FaceDirection(bool faceleft, float speedMultiplier, bool immediate = false)
        {
            float xScale = faceleft ? 1 : -1;
            Vector3 newScale = new Vector3 (xScale, 1, 1);

            if (!immediate)
            {
                Image newRenderer =  CreateRenderer(renderer.transform.parent);

                newRenderer.transform.localScale = newScale;

                transitionSpeedMultiplier = speedMultiplier;
                TryStartLevelingAlpha();

                while (isLevelingAlpha)
                    yield return null;

            }
            else
            {
                renderer.transform.localScale = newScale;
            }
            co_Flipping = null;
        }
    }
}
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityHelpers
{
    /// <summary>
    /// A component for UI that lets you set up a border
    /// </summary>
    [ExecuteInEditMode, RequireComponent(typeof(RectTransform))]
    public class BorderController : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private RectTransform rectTransform { get { if (!_rectTransform) _rectTransform = GetComponent<RectTransform>(); return _rectTransform; } }

        [Flags]
        public enum BorderEdge { left = 0x1, right = 0x2, top = 0x4, bottom = 0x8, topLeft = 0x10, topRight = 0x20, bottomLeft = 0x40, bottomRight = 0x80, }

        public static readonly Vector2[] pivots = new Vector2[] { Vector2.left, Vector2.right, Vector2.up, Vector2.down, new Vector2(-1, 1), new Vector2(1, 1), new Vector2(-1, -1), new Vector2(1, -1) };
        [SerializeField]
        private Image[] borderImages;

        [EnumFlags]
        public BorderEdge shownEdges = BorderEdge.left | BorderEdge.right | BorderEdge.top | BorderEdge.bottom | BorderEdge.topLeft | BorderEdge.topRight | BorderEdge.bottomLeft | BorderEdge.bottomRight;
        private BorderEdge _prevShownEdges;

        public bool dependant;
        private bool _prevDependant;
        public float borderSize;
        private float _prevBorderSize;
        public Color borderColor;
        private Color _prevBorderColor;

        private Vector2 _prevSize;

        public event OnValueChanged onEdgesShownChanged, onSizeChanged, onBorderSizeChanged, onDependantChanged, onBorderColorChanged;
        public delegate void OnValueChanged();

        void Awake()
        {
            InitPrev();
        }
        void Update()
        {
            CheckBorderImages();
            CheckSizeChanged();
            SetShownEdges(shownEdges);
            SetDependant(dependant);
            SetBorderSize(borderSize);
            SetBorderColor(borderColor);
        }
        void Reset()
        {
            CheckBorderImages();
            SetShownEdges(BorderEdge.bottom | BorderEdge.bottomLeft | BorderEdge.bottomRight | BorderEdge.left | BorderEdge.right | BorderEdge.top | BorderEdge.topLeft | BorderEdge.topRight);
            SetDependant(false);
            SetBorderSize(2);
            SetBorderColor(Color.black);
            ResetBorders();
        }

        private void InitPrev()
        {
            _prevShownEdges = shownEdges;
            _prevBorderSize = borderSize;
            _prevDependant = dependant;
            _prevBorderColor = borderColor;
            //_prevSize = rectTransform.rect.size;
        }
        private void CheckSizeChanged()
        {
            if (!Mathf.Approximately(rectTransform.rect.width, _prevSize.x) || !Mathf.Approximately(rectTransform.rect.height, _prevSize.y))
            {
				if (onSizeChanged != null)
                	onSizeChanged.Invoke();

                ResetBorders();
                _prevSize = rectTransform.rect.size;
            }
        }

        public void SetShownEdges(BorderEdge value)
        {
            shownEdges = value;

            if (shownEdges != _prevShownEdges)
            {
				if (onEdgesShownChanged != null)
                	onEdgesShownChanged.Invoke();

                ResetBorders();

                _prevShownEdges = shownEdges;
            }
        }
        public void SetBorderSize(float value)
        {
            if (dependant)
                borderSize = Mathf.Clamp01(value);
            else
                borderSize = Mathf.Abs(value);

            if (borderSize != _prevBorderSize)
            {
				if (onBorderSizeChanged != null)
                	onBorderSizeChanged.Invoke();

                ResetBorders();
                _prevBorderSize = borderSize;
            }
        }
        public void SetDependant(bool value)
        {
            dependant = value;

            if (dependant != _prevDependant)
            {
				if (onDependantChanged != null)
                	onDependantChanged.Invoke();

                if (dependant)
                    SetBorderSize(borderSize / Mathf.Min(rectTransform.rect.width, rectTransform.rect.height));
                else
                    SetBorderSize(borderSize * Mathf.Min(rectTransform.rect.width, rectTransform.rect.height));

                _prevDependant = dependant;
            }
        }
        public void SetBorderColor(Color value)
        {
            borderColor = value;

            foreach (Image borderImage in borderImages)
                borderImage.color = borderColor;

            if (borderColor.r != _prevBorderColor.r || borderColor.g != _prevBorderColor.g || borderColor.b != _prevBorderColor.b || borderColor.a != _prevBorderColor.a)
            {
				if (onBorderColorChanged != null)
                	onBorderColorChanged.Invoke();

                _prevBorderColor = borderColor;
            }
        }

        public void ResetBorders()
        {
            float calculatedSize = dependant ? borderSize * Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) : borderSize;
            float longWidth = rectTransform.rect.width - (calculatedSize * 2), longHeight = rectTransform.rect.height - (calculatedSize * 2);
            for (int i = 0; i < borderImages.Length; i++)
            {
                Image borderImage = borderImages[i];
                if (!borderImage)
                    continue;

                BorderEdge borderPosition = (BorderEdge)Mathf.RoundToInt(Mathf.Pow(2, i));
                bool isVisible = (shownEdges & borderPosition) != 0;
                borderImage.gameObject.SetActive(isVisible);

                Vector2 currentSize;
                if (borderPosition == BorderEdge.left || borderPosition == BorderEdge.right)
                    currentSize = new Vector2(calculatedSize, longHeight);
                else if (borderPosition == BorderEdge.top || borderPosition == BorderEdge.bottom)
                    currentSize = new Vector2(longWidth, calculatedSize);
                else
                    currentSize = new Vector2(calculatedSize, calculatedSize);

                borderImage.rectTransform.sizeDelta = currentSize;

                Vector2 borderPivot = pivots[i];
                borderImage.rectTransform.localPosition = new Vector2(borderPivot.x * rectTransform.rect.width / 2f - borderPivot.x * currentSize.x / 2f, borderPivot.y * rectTransform.rect.height / 2f - borderPivot.y * currentSize.y / 2f);
            }
        }

        private void CheckBorderImages()
        {
            if (borderImages == null || borderImages.Length != 8 || borderImages.Any(image => image == null))
                ResetBorderImages();
        }
        private void ResetBorderImages()
        {
            DestroyBorderImages();
            CreateBorderImages();
        }
        private void DestroyBorderImages()
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in transform)
                children.Add(child);
            for (int i = children.Count - 1; i >= 0; i--)
                DestroyImmediate(children[i].gameObject);

            borderImages = null;
        }
        private void CreateBorderImages()
        {
            borderImages = new Image[8];
            for (int i = 0; i < borderImages.Length; i++)
            {
                GameObject borderImageObj = new GameObject("BorderImage" + i);
                borderImageObj.transform.SetParent(transform, false);
                Image currentBorderImage = borderImageObj.AddComponent<Image>();
                borderImages[i] = currentBorderImage;
            }
        }
    }
}
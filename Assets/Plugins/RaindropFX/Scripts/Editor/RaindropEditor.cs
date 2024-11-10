using UnityEditor;
using UnityEditor.Rendering;

namespace NKStudio
{
    [CustomEditor(typeof(Raindrop))]
    public sealed class RaindropEditor : VolumeComponentEditor
    {
        private SerializedDataParameter _rainDropAnimationTime;
        private SerializedDataParameter _intensity;
        private SerializedDataParameter _aspect;
        private SerializedDataParameter _size;
        private SerializedDataParameter _wiggleStrength;
        private SerializedDataParameter _staticRaindropAnimationTime;
        private SerializedDataParameter _staticRainIntensity;
        private SerializedDataParameter _staticRaindropAmount;
        private SerializedDataParameter _staticRaindropSize;
        private SerializedDataParameter _fogEnable;

        private SerializedDataParameter _fogIntensity;

        // private SerializedDataParameter _blurEnable;
        // private SerializedDataParameter _blurIntensity;
        private SerializedDataParameter _zoom;

        public override void OnEnable()
        {
            var o = new PropertyFetcher<Raindrop>(serializedObject);

            _rainDropAnimationTime = Unpack(o.Find(x => x.RainDropAnimationTime));
            _intensity = Unpack(o.Find(x => x.Intensity));
            _aspect = Unpack(o.Find(x => x.Aspect));
            _size = Unpack(o.Find(x => x.Size));
            _wiggleStrength = Unpack(o.Find(x => x.WiggleStrength));
            _staticRaindropAnimationTime = Unpack(o.Find(x => x.StaticRaindropAnimationTime));
            _staticRainIntensity = Unpack(o.Find(x => x.StaticRainIntensity));
            _staticRaindropAmount = Unpack(o.Find(x => x.StaticRaindropAmount));
            _staticRaindropSize = Unpack(o.Find(x => x.StaticRaindropSize));
            _fogEnable = Unpack(o.Find(x => x.FogEnable));
            _fogIntensity = Unpack(o.Find(x => x.FogIntensity));
            // _blurEnable = Unpack(o.Find(x => x.BlurEnable));
            // _blurIntensity = Unpack(o.Find(x => x.BlurIntensity));
            _zoom = Unpack(o.Find(x => x.Zoom));
        }

        public override void OnInspectorGUI()
        {
            PropertyField(_rainDropAnimationTime, EditorGUIUtility.TrTextContent("Animation Time", "빗방울이 움직이는 시간 속도입니다."));
            PropertyField(_intensity, EditorGUIUtility.TrTextContent("Intensity", "빗방울의 강도입니다."));
            PropertyField(_aspect, EditorGUIUtility.TrTextContent("Aspect", "빗방울의 가로 세로 비율입니다."));
            PropertyField(_size, EditorGUIUtility.TrTextContent("Size", "빗방울의 크기입니다."));
            PropertyField(_wiggleStrength, EditorGUIUtility.TrTextContent("Wiggle Strength", "빗방울의 흔들림 강도입니다."));
            PropertyField(_staticRaindropAnimationTime, EditorGUIUtility.TrTextContent("Animation Time", "정적 빗방울이 움직이는 시간 속도입니다."));
            PropertyField(_staticRainIntensity, EditorGUIUtility.TrTextContent("Intensity", "정적 빗방울의 강도입니다."));
            PropertyField(_staticRaindropAmount, EditorGUIUtility.TrTextContent("Amount", "정적 빗방울의 양입니다."));
            PropertyField(_staticRaindropSize, EditorGUIUtility.TrTextContent("Size", "정적 빗방울의 크기입니다."));
            PropertyField(_fogEnable, EditorGUIUtility.TrTextContent("Enable", "안개 효과를 사용할지 여부입니다."));
            PropertyField(_fogIntensity, EditorGUIUtility.TrTextContent("Intensity", "안개 효과의 강도입니다."));
            // PropertyField(_blurEnable, EditorGUIUtility.TrTextContent("Enable", "흐림 효과를 사용할지 여부입니다."));
            // PropertyField(_blurIntensity, EditorGUIUtility.TrTextContent("Intensity", "흐림 효과의 강도입니다."));
            PropertyField(_zoom, EditorGUIUtility.TrTextContent("Zoom", "줌 효과의 강도입니다."));
        }
    }
}
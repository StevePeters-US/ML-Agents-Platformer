using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;
using UnityEngine.UI;

namespace APG {
    [AddComponentMenu("ML Agents/Texture Sensor")]
    public class TextureSensorComponent : SensorComponent {
        TextureSensor m_Sensor;

        [SerializeField] Texture2D tex;
        [SerializeField] RawImage debugImage;
        public Texture2D Tex { get => tex; set => tex = value; }

        [SerializeField] private Vector2Int textureSize = new Vector2Int(10, 10);

        [SerializeField]
        string m_SensorName = "TextureSensor";

        // Note that changing this at runtime does not affect how the Agent sorts the sensors.
        public string SensorName {
            get { return m_SensorName; }
            set { m_SensorName = value; }
        }

        [SerializeField] bool m_Grayscale;

        // Whether the Texture observation should be converted to grayscale or not.
        // Note that changing this after the sensor is created has no effect.
        public bool Grayscale {
            get { return m_Grayscale; }
            set { m_Grayscale = value; }
        }

        [SerializeField]
        [Range(1, 50)]
        [Tooltip("Number of frames that will be stacked before being fed to the neural network.")]
        int m_ObservationStacks = 1;

        [SerializeField]
        SensorCompressionType m_Compression = SensorCompressionType.PNG;

        // Compression type for the render texture observation.
        public SensorCompressionType CompressionType {
            get { return m_Compression; }
            set { m_Compression = value; UpdateSensor(); }
        }

        // Whether to stack previous observations. Using 1 means no previous observations.
        // Note that changing this after the sensor is created has no effect.
        public int ObservationStacks {
            get { return m_ObservationStacks; }
            set { m_ObservationStacks = value; }
        }
        private void Awake() {
            if (tex == null)
                tex = new Texture2D(textureSize.x, textureSize.y, TextureFormat.RGB24, false);

            tex.Resize(textureSize.x, textureSize.y);

            if (debugImage)
                debugImage.texture = tex;
        }

        public override ISensor[] CreateSensors() {
            Dispose();

            m_Sensor = new TextureSensor(Tex, Grayscale, SensorName, CompressionType);
            if (ObservationStacks != 1) {
                return new ISensor[] { new StackingSensor(m_Sensor, ObservationStacks) };
            }
            return new ISensor[] { m_Sensor };
        }

        /// Update fields that are safe to change on the Sensor at runtime.
        internal void UpdateSensor() {
            if (m_Sensor != null) {
                m_Sensor.CompressionType = m_Compression;
            }
        }

        /// Clean up the sensor created by CreateSensors().
        public void Dispose() {
            if (!ReferenceEquals(null, m_Sensor)) {
                m_Sensor.Dispose();
                m_Sensor = null;
            }
        }
    }
}

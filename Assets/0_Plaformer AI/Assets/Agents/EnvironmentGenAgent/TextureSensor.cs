using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.MLAgents.Sensors;

namespace APG {
    public class TextureSensor : ISensor, IDisposable {


        // RenderTexture m_RenderTexture;
        bool m_Grayscale;
        string m_Name;
        private ObservationSpec m_ObservationSpec;
        SensorCompressionType m_CompressionType;
        Texture2D m_Texture;


        public SensorCompressionType CompressionType {
            get { return m_CompressionType; }
            set { m_CompressionType = value; }
        }

        public TextureSensor(Texture2D tex, bool grayscale, string name, SensorCompressionType compressionType) {
            //m_RenderTexture = renderTexture;
            var width = tex != null ? tex.width : 0;
            var height = tex != null ? tex.height : 0;
            m_Grayscale = grayscale;
            m_Name = name;
            m_ObservationSpec = ObservationSpec.Visual(height, width, grayscale ? 1 : 3);
            m_CompressionType = compressionType;
            m_Texture = tex;// new Texture2D(width, height, TextureFormat.RGB24, false);
        }

        public string GetName() {
            return m_Name;
        }

        public ObservationSpec GetObservationSpec() {
            return m_ObservationSpec;
        }

        /// <inheritdoc/>
        public byte[] GetCompressedObservation() {
            //using (TimerStack.Instance.Scoped("RenderTextureSensor.GetCompressedObservation")) {
            // ObservationToTexture(m_RenderTexture, m_Texture);
            // TODO support more types here, e.g. JPG
            var compressed = m_Texture.EncodeToPNG();
            return compressed;
        }

        /// <inheritdoc/>
        public int Write(ObservationWriter writer) {
            //using (TimerStack.Instance.Scoped("RenderTextureSensor.Write")) {
            //   ObservationToTexture(m_RenderTexture, m_Texture);
            var numWritten = writer.WriteTexture(m_Texture, m_Grayscale);
            return numWritten;
        }

        /// <inheritdoc/>
        public void Update() { }

        /// <inheritdoc/>
        public void Reset() { }

        /// <inheritdoc/>
        public CompressionSpec GetCompressionSpec() {
            return new CompressionSpec(m_CompressionType);
        }

        /// <inheritdoc/>
        public BuiltInSensorType GetBuiltInSensorType() {
            return BuiltInSensorType.RenderTextureSensor;
        }

        /// <summary>
        /// Converts a RenderTexture to a 2D texture.
        /// </summary>
        /// <param name="obsTexture">RenderTexture.</param>
        /// <param name="texture2D">Texture2D to render to.</param>
        public static void ObservationToTexture(RenderTexture obsTexture, Texture2D texture2D) {
            var prevActiveRt = RenderTexture.active;
            RenderTexture.active = obsTexture;

            texture2D.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = prevActiveRt;
        }

        /// <summary>
        /// Clean up the owned Texture2D.
        /// </summary>
        public void Dispose() {
            /*if (!ReferenceEquals(null, m_Texture)) {
                Utilities.DestroyTexture(m_Texture);
                m_Texture = null;
            }*/
        }
    }
}

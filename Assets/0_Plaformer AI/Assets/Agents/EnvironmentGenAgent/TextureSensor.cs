using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.MLAgents.Sensors;

namespace APG {
    // Mostly copied from the render texture sensor
    public class TextureSensor : ISensor, IDisposable {

        bool m_Grayscale;
        string m_Name;
        private ObservationSpec m_ObservationSpec;
        SensorCompressionType m_CompressionType;
        Texture2D m_Texture;
        Texture2D runtimeTex;


        public SensorCompressionType CompressionType {
            get { return m_CompressionType; }
            set { m_CompressionType = value; }
        }

        public TextureSensor(Texture2D tex, bool grayscale, string name, SensorCompressionType compressionType) {
            var width = tex != null ? tex.width : 0;
            var height = tex != null ? tex.height : 0;
            m_Grayscale = grayscale;
            m_Name = name;
            m_ObservationSpec = ObservationSpec.Visual(height, width, grayscale ? 1 : 3);
            m_CompressionType = compressionType;
            m_Texture = tex;
            //runtimeTex = new Texture2D(width, height);
        }

        public string GetName() {
            return m_Name;
        }

        public ObservationSpec GetObservationSpec() {
            return m_ObservationSpec;
        }

        public byte[] GetCompressedObservation() {
            var compressed = m_Texture.EncodeToPNG();
            //var compressed = runtimeTex.EncodeToPNG();
            return compressed;
        }

        public int Write(ObservationWriter writer) {
            var numWritten = writer.WriteTexture(m_Texture, m_Grayscale);
           // var numWritten = writer.WriteTexture(runtimeTex, m_Grayscale);
            return numWritten;
        }

        public void Update() { }

        public void Reset() { }

        public CompressionSpec GetCompressionSpec() {
            return new CompressionSpec(m_CompressionType);
        }

        public BuiltInSensorType GetBuiltInSensorType() {
            return BuiltInSensorType.RenderTextureSensor;
        }

        /// Clean up the owned Texture2D.
        public void Dispose() {
            if (!ReferenceEquals(null, m_Texture)) {

             /*   if (Application.isEditor) {
                    // Edit Mode tests complain if we use Destroy()
                    UnityEngine.Object.DestroyImmediate(m_Texture);
                }
                else {
                    UnityEngine.Object.Destroy(m_Texture);
                }*/

                m_Texture = null;
                //runtimeTex = null;
            }
        }
    }
}

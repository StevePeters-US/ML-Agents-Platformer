using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;

namespace APG.Environment {
    [AddComponentMenu("ML Agents/Grid 3D Sensor")]
    public class Grid3DSensorComponent : SensorComponent {
        protected Grid3DSensor m_Sensor;

        [HideInInspector, SerializeField]
        internal string m_SensorName = "Grid3DSensor";
        // <summary>
        /// Name of the generated <see cref="GridSensor"/> object.
        /// Note that changing this at runtime does not affect how the Agent sorts the sensors.
        /// </summary>
        public string SensorName {
            get { return m_SensorName; }
            set { m_SensorName = value; }
        }

        [HideInInspector, SerializeField]
        internal Vector3Int m_GridSize = new Vector3Int(10, 1, 10);

        public Vector3Int GridSize {
            get { return m_GridSize; }
            set {
                if (value.y != 1) {
                    m_GridSize = new Vector3Int(value.x, 1, value.z);
                }
                else {
                    m_GridSize = value;
                }
            }
        }

        [SerializeField] internal SensorCompressionType m_CompressionType = SensorCompressionType.PNG;

        /// Whether to stack previous observations. Using 1 means no previous observations.
        /// Note that changing this after the sensor is created has no effect.
        [SerializeField, Range(1, 50)]
        [Tooltip("Number of frames of observations that will be stacked before being fed to the neural network.")]
        internal int m_ObservationStacks = 1;
        public int ObservationStacks {
            get { return m_ObservationStacks; }
            set { m_ObservationStacks = value; }
        }

        public override ISensor[] CreateSensors() {
            var board = GetComponent<Grid3D_Abstract>();
            var cellSensor = Grid3DSensor.CellTypeSensor(board, m_SensorName + " (cells)", m_CompressionType);
            // This can be null if numSpecialTypes is 0
            /*      var specialSensor = Match3Sensor.SpecialTypeSensor(board, ObservationType, SensorName + " (special)");
                  return specialSensor != null ? new ISensor[] { cellSensor, specialSensor } : new ISensor[] { cellSensor };*/

            return new ISensor[] { cellSensor };
        }

        /*        public override ISensor[] CreateSensors() {
                    m_Sensor = new Grid3DSensor(string m_SensorName, Grid_3D board, Vector3Int gridSize, GridValueProvider gvp, int oneHotSize, SensorCompressionType m_CompressionType
                      m_SensorName,
                      //m_CellScale,
                      m_GridSize,
                      //m_ChannelDepths,
                      //m_DetectableObjects,
                      // m_DepthType,
                      // RootReference,
                      m_CompressionType
                  // m_MaxColliderBufferSize,
                  // m_InitialColliderBufferSize
                  );

                    if (ObservationStacks != 1) {
                        return new ISensor[] { new StackingSensor(m_Sensor, ObservationStacks) };
                    }
                    return new ISensor[] { m_Sensor };
                }*/


    }
}

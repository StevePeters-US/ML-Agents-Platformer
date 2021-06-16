using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;

namespace APG.Environment {
    /// Delegate that provides integer values at a given (x,y) coordinate.
    public delegate int GridValueProvider(int x, int y);
    public class Grid3DSensor : ISensor {


        ObservationSpec m_ObservationSpec;
        string m_SensorName;

        Grid3D_Abstract m_Board;
        //Grid3DPropertiesStruct m_MaxBoardSize;
        Vector3Int gridSize;
        GridValueProvider m_GridValues;
        int m_OneHotSize;

        SensorCompressionType m_sensorCompressionType;

        public Grid3DSensor(string m_SensorName, Grid3D_Abstract board, GridValueProvider gvp, int oneHotSize,SensorCompressionType m_CompressionType) {
            //var maxBoardSize = board.GetMaxBoardSize();
            this.m_SensorName = m_SensorName;
            //m_MaxBoardSize = maxBoardSize;
            this.gridSize = board.CurrentGrid3DProperties.GridSize;// .GridSize;// gridSize;
            m_GridValues = gvp;
            m_OneHotSize = oneHotSize;
            m_Board = board;

          //  m_ObservationSpec = ObservationSpec.Visual(maxBoardSize.Rows, maxBoardSize.Columns, oneHotSize);
            m_ObservationSpec = ObservationSpec.Visual(gridSize.x, gridSize.z, oneHotSize);

            this.m_sensorCompressionType = m_CompressionType;
        }

      /// <summary>
        /// Create a sensor that encodes the board cells as observations.
        /// </summary>
        /// <param name="board">The abstract board.</param>
        /// <param name="obsType">Whether to produce vector or visual observations</param>
        /// <param name="name">Name of the sensor.</param>
        /// <returns></returns>
        public static Grid3DSensor CellTypeSensor(Grid3D_Abstract board,string name, SensorCompressionType sensorCompressionType) {
            var maxBoardSize = board.GetMaxBoardSize();
            return new Grid3DSensor(name, board, board.GetCellType, maxBoardSize.NumCellTypes, sensorCompressionType);
        }
/*
        /// <summary>
        /// Create a sensor that encodes the cell special types as observations. Returns null if the board's
        /// NumSpecialTypes is 0 (indicating the sensor isn't needed).
        /// </summary>
        /// <param name="board">The abstract board.</param>
        /// <param name="obsType">Whether to produce vector or visual observations</param>
        /// <param name="name">Name of the sensor.</param>
        /// <returns></returns>
        public static Match3Sensor SpecialTypeSensor(AbstractBoard board, Match3ObservationType obsType, string name) {
            var maxBoardSize = board.GetMaxBoardSize();
            if (maxBoardSize.NumSpecialTypes == 0) {
                return null;
            }
            var specialSize = maxBoardSize.NumSpecialTypes + 1;
            return new Match3Sensor(board, board.GetSpecialType, specialSize, obsType, name);
        }*/

        /// <inheritdoc/>
        public ObservationSpec GetObservationSpec() {
            return m_ObservationSpec;
        }

        /// <inheritdoc/>
        public int Write(ObservationWriter writer) {
            //m_Board.CheckBoardSizes(m_MaxBoardSize);
            var currentBoardSize = m_Board.CurrentGrid3DProperties.GridSize;// GetCurrentBoardSize();

            int offset = 0;
            var isVisual = true;// m_ObservationType != Match3ObservationType.Vector;

            // This is equivalent to
            // for (var r = 0; r < m_MaxBoardSize.Rows; r++)
            //     for (var c = 0; c < m_MaxBoardSize.Columns; c++)
            //          if (r < currentBoardSize.Rows && c < currentBoardSize.Columns)
            //              WriteOneHot
            //          else
            //              WriteZero
            // but rearranged to avoid the branching.

            for (var r = 0; r < currentBoardSize.x; r++) {
                for (var c = 0; c < currentBoardSize.z; c++) {
                    var val = m_GridValues(r, c);
                    writer.WriteOneHot(offset, r, c, val, m_OneHotSize, isVisual);
                    offset += m_OneHotSize;
                }

                for (var c = currentBoardSize.z; c < m_MaxBoardSize.Columns; c++) {
                    writer.WriteZero(offset, r, c, m_OneHotSize, isVisual);
                    offset += m_OneHotSize;
                }
            }

            for (var r = currentBoardSize.x; r < m_MaxBoardSize.Columns; r++) {
                for (var c = 0; c < m_MaxBoardSize.Columns; c++) {
                    writer.WriteZero(offset, r, c, m_OneHotSize, isVisual);
                    offset += m_OneHotSize;
                }
            }


            return offset;

        }

        public byte[] GetCompressedObservation() {
            //m_Board.CheckBoardSizes(m_MaxBoardSize);
            var height = m_MaxBoardSize.Rows;
            var width = m_MaxBoardSize.Columns;
            var tempTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
            var converter = new OneHotToTextureUtil(height, width);
            var bytesOut = new List<byte>();
            var currentBoardSize = m_Board.CurrentGrid3DProperties.GridSize;// GetCurrentBoardSize();

            // Encode the cell types or special types as batches of PNGs
            // This is potentially wasteful, e.g. if there are 4 cell types and 1 special type, we could
            // fit in in 2 images, but we'll use 3 total (2 PNGs for the 4 cell type channels, and 1 for
            // the special types).
            var numCellImages = (m_OneHotSize + 2) / 3;
            for (var i = 0; i < numCellImages; i++) {
                converter.EncodeToTexture(
                    m_GridValues,
                    tempTexture,
                    3 * i,
                    currentBoardSize.x,
                    currentBoardSize.z
                );
                bytesOut.AddRange(tempTexture.EncodeToPNG());
            }

            DestroyTexture(tempTexture);
            return bytesOut.ToArray();
        }

        public CompressionSpec GetCompressionSpec() {
            return new CompressionSpec(m_sensorCompressionType);
        }

        public string GetName() {
            return m_SensorName;
        }

        public BuiltInSensorType GetBuiltInSensorType() {
            return BuiltInSensorType.Match3Sensor;
        }

        static void DestroyTexture(Texture2D texture) {
            if (Application.isEditor) {
                // Edit Mode tests complain if we use Destroy()
                Object.DestroyImmediate(texture);
            }
            else {
                Object.Destroy(texture);
            }
        }

        public void Update() {
            throw new System.NotImplementedException();
        }

        public void Reset() {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// Utility class for converting a 2D array of ints representing a one-hot encoding into
    /// a texture, suitable for conversion to PNGs for observations.
    /// Works by encoding 3 values at a time as pixels in the texture, thus it should be
    /// called (maxValue + 2) / 3 times, increasing the channelOffset by 3 each time.
    /// </summary>
    internal class OneHotToTextureUtil {
        Color[] m_Colors;
        int m_MaxHeight;
        int m_MaxWidth;
        private static Color[] s_OneHotColors = { Color.red, Color.green, Color.blue };

        public OneHotToTextureUtil(int maxHeight, int maxWidth) {
            m_Colors = new Color[maxHeight * maxWidth];
            m_MaxHeight = maxHeight;
            m_MaxWidth = maxWidth;
        }

        public void EncodeToTexture(
            GridValueProvider gridValueProvider,
            Texture2D texture,
            int channelOffset,
            int currentHeight,
            int currentWidth
            ) {
            var i = 0;
            // There's an implicit flip converting to PNG from texture, so make sure we
            // counteract that when forming the texture by iterating through h in reverse.
            for (var h = m_MaxHeight - 1; h >= 0; h--) {
                for (var w = 0; w < m_MaxWidth; w++) {
                    var colorVal = Color.black;
                    if (h < currentHeight && w < currentWidth) {
                        int oneHotValue = gridValueProvider(h, w);
                        if (oneHotValue >= channelOffset && oneHotValue < channelOffset + 3) {
                            colorVal = s_OneHotColors[oneHotValue - channelOffset];
                        }
                    }
                    m_Colors[i++] = colorVal;
                }
            }
            texture.SetPixels(m_Colors);
        }
    }

    /// <summary>
    /// Utility methods for writing one-hot observations.
    /// </summary>
    internal static class ObservationWriterMatch3Extensions {
        public static void WriteOneHot(this ObservationWriter writer, int offset, int row, int col, int value, int oneHotSize, bool isVisual) {
            if (isVisual) {
                for (var i = 0; i < oneHotSize; i++) {
                    writer[row, col, i] = (i == value) ? 1.0f : 0.0f;
                }
            }
            else {
                for (var i = 0; i < oneHotSize; i++) {
                    writer[offset] = (i == value) ? 1.0f : 0.0f;
                    offset++;
                }
            }
        }

        public static void WriteZero(this ObservationWriter writer, int offset, int row, int col, int oneHotSize, bool isVisual) {
            if (isVisual) {
                for (var i = 0; i < oneHotSize; i++) {
                    writer[row, col, i] = 0.0f;
                }
            }
            else {
                for (var i = 0; i < oneHotSize; i++) {
                    writer[offset] = 0.0f;
                    offset++;
                }
            }
        }
    }
}


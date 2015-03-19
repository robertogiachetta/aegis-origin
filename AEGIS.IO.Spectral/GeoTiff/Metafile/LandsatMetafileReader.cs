﻿/// <copyright file="LandsatMetafileReader.cs" company="Eötvös Loránd University (ELTE)">
///     Copyright (c) 2011-2015 Roberto Giachetta. Licensed under the
///     Educational Community License, Version 2.0 (the "License"); you may
///     not use this file except in compliance with the License. You may
///     obtain a copy of the License at
///     http://opensource.org/licenses/ECL-2.0
///
///     Unless required by applicable law or agreed to in writing,
///     software distributed under the License is distributed on an "AS IS"
///     BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
///     or implied. See the License for the specific language governing
///     permissions and limitations under the License.
/// </copyright>
/// <author>Tamás Kovács</author>
/// <author>Roberto Giachetta</author>

using ELTE.AEGIS.Reference;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ELTE.AEGIS.IO.GeoTiff.Metafile
{
    /// <summary>
    /// Represents a type for reading Landsat metafiles.
    /// </summary>
    public class LandsatMetafileReader : GeoTiffMetafileReader
    {
        #region Private fields

        /// <summary>
        /// The dictionary containing the metadata.
        /// </summary>
        private Dictionary<String, String> _metadata;

        #endregion

        #region Protected GeoTiffMetafileReader properties

        /// <summary>
        /// Gets the default extension of the metafile.
        /// </summary>
        protected override String DefaultExtension
        {
            get { return "txt"; }
        }

        /// <summary>
        /// Gets the default file name of the metafile.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override String DefaultFileName
        {
            get { return "metadata.txt"; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LandsatMetafileReader" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="option">The path option.</param>
        /// <exception cref="System.ArgumentNullException">The path is null.</exception>
        /// <exception cref="System.ArgumentException">
        /// The path is empty.
        /// or
        /// The path is invalid.
        /// or
        /// The path is a zero-length string, contains only white space, or contains one or more invalid characters.
        /// or
        /// The path, file name, or both exceed the system-defined maximum length.
        /// </exception>
        /// <exception cref="System.UnauthorizedAccessException">
        /// The file on path is hidden.
        /// or
        /// The file on path is read-only.
        /// or
        /// The caller does not have the required permission for the path.
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">The metafile does not exist.</exception>
        public LandsatMetafileReader(String path, GeoTiffMetafilePathOption option)
            : base(path, option)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LandsatMetafileReader" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="option">The path option.</param>
        /// <exception cref="System.ArgumentNullException">The path is null.</exception>
        /// <exception cref="System.ArgumentException">
        /// The path is empty.
        /// or
        /// The path is invalid.
        /// or
        /// The path is a zero-length string, contains only white space, or contains one or more invalid characters.
        /// or
        /// The path, file name, or both exceed the system-defined maximum length.
        /// </exception>
        /// <exception cref="System.UnauthorizedAccessException">
        /// The file on path is hidden.
        /// or
        /// The file on path is read-only.
        /// or
        /// The caller does not have the required permission for the path.
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">The metafile does not exist.</exception>
        public LandsatMetafileReader(Uri path, GeoTiffMetafilePathOption option)
            : base(path, option)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LandsatMetafileReader"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <exception cref="System.ArgumentNullException">The stream is null.</exception>
        public LandsatMetafileReader(Stream stream)
            : base(stream)
        {
        }

        #endregion

        #region Protected GeoTiffMetafileReader methods

        /// <summary>
        /// Reads the device information stored in the metafile stream.
        /// </summary>
        /// <returns>The device data.</returns>
        protected override ImagingDevice ReadDeviceFromStream()
        {
            ReadContent();

            return ImagingDevices.FromName(_metadata["SPACECRAFT_ID"].Replace("_", String.Empty) + " " + _metadata["SENSOR_ID"]).FirstOrDefault();
        }

        /// <summary>
        /// Reads the imaging information stored in the metafile stream.
        /// </summary>
        /// <returns>The imaging data.</returns>
        protected override RasterImaging ReadImagingFromStream()
        {
            ReadContent();

            // read the device data
            ImagingDevice device = ReadDeviceFromStream();

            // time            
            DateTime imagingDateTime = DateTime.Parse(_metadata["DATE_ACQUIRED"] + " " + _metadata["SCENE_CENTER_TIME"], CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal);

            // view
            Double incidenceAngle = Double.NaN;
            Double viewingAngle = _metadata.ContainsKey("ROLL_ANGLE") ? Double.Parse(_metadata["ROLL_ANGLE"], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat) : Double.NaN; // only Landsat 8 contains the roll angle property
            Double sunAzimuth = Double.Parse(_metadata["SUN_AZIMUTH"], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat);
            Double sunElevation = Double.Parse(_metadata["SUN_ELEVATION"], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat);

            // image location
            GeoCoordinate[] imageLocation = new GeoCoordinate[]
            {
                new GeoCoordinate(Double.Parse(_metadata["CORNER_UL_LAT_PRODUCT"], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat), 
                                  Double.Parse(_metadata["CORNER_UL_LON_PRODUCT"], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat)),
                new GeoCoordinate(Double.Parse(_metadata["CORNER_UR_LAT_PRODUCT"], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat), 
                                  Double.Parse(_metadata["CORNER_UR_LON_PRODUCT"], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat)),
                new GeoCoordinate(Double.Parse(_metadata["CORNER_LL_LAT_PRODUCT"], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat), 
                                  Double.Parse(_metadata["CORNER_LL_LON_PRODUCT"], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat)),
                new GeoCoordinate(Double.Parse(_metadata["CORNER_LR_LAT_PRODUCT"], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat), 
                                  Double.Parse(_metadata["CORNER_LR_LON_PRODUCT"], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat))
            };

            // band parameters
            List<RasterImagingBand> bandData = new List<RasterImagingBand>();

            Int32 numberOfBands = 0;
            Double[] solarIrradiance = null;

            switch (device.MissionNumber)
            {
                case 7:
                    numberOfBands = 8;
                    // solar irrandiance is constant for Landsat 7, see: http://landsathandbook.gsfc.nasa.gov/pdfs/Landsat_Calibration_Summary_RSE.pdf
                    solarIrradiance =  new Double[] { 1997, 1812, 1533, 1039, 230.8, Double.NaN, 84.9, 1362 };
                    break;
                case 8:
                    numberOfBands = 11;
                    // solar irrance is not provided for Landsat 8, see: http://landsat.usgs.gov/ESUN.php
                    solarIrradiance = Enumerable.Repeat(Double.NaN, 11).ToArray();
                    break;
            }

            for (Int32 bandIndex = 0; bandIndex < numberOfBands; bandIndex++)
            {
                String indexString = (bandIndex + 1).ToString();
                if (device.MissionNumber == 7 && bandIndex == 5) // Landsat 7 band 6 has high gain and low gain modes
                    indexString += "_VCID_1";

                Double maximumRadiance = Double.Parse(_metadata["RADIANCE_MAXIMUM_BAND_" + indexString], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat);
                Double minimumRadiance = Double.Parse(_metadata["RADIANCE_MINIMUM_BAND_" + indexString], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat);
                Double qCalMax = Double.Parse(_metadata["QUANTIZE_CAL_MAX_BAND_" + indexString], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat);
                Double qCalMin = Double.Parse(_metadata["QUANTIZE_CAL_MIN_BAND_" + indexString], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat);


                Double physicalGain = (maximumRadiance - minimumRadiance) / (qCalMax - qCalMin);
                Double physicalBias = minimumRadiance;

                // match the device band data
                ImagingDeviceBand deviceBand = null;

                if (device != null)
                    deviceBand = device.Bands.FirstOrDefault(band => band.Description.Contains("BAND " + bandIndex));

                if (deviceBand != null)
                    bandData.Add(new RasterImagingBand(deviceBand.Description, physicalGain, physicalBias, solarIrradiance[bandIndex], deviceBand.SpectralDomain, deviceBand.SpectralRange));
                else // if no match is found
                    bandData.Add(new RasterImagingBand("BAND " + bandIndex, physicalGain, physicalBias, solarIrradiance[bandIndex], SpectralDomain.Undefined, null));
            }

            return new RasterImaging(device, imagingDateTime, GeoCoordinate.Undefined, imageLocation, incidenceAngle, viewingAngle, sunAzimuth, sunElevation, bandData);
        }

        /// <summary>
        /// Reads the reference system stored in the metafile stream.
        /// </summary>
        /// <returns>The reference system.</returns>
        protected override IReferenceSystem ReadReferenceSystemFromStream()
        {
            ReadContent();

            return Geographic2DCoordinateReferenceSystems.FromName(_metadata["DATUM"]).FirstOrDefault();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Reads the content of the file.
        /// </summary>
        private void ReadContent()
        {
            if (_metadata != null)
                return;

            _metadata = new Dictionary<String, String>();

            StreamReader reader = new StreamReader(_stream);

            String line;
            while ((line = reader.ReadLine()) != null && line != "END")
            {
                String[] splitLine = line.Trim().Split(new String[] { " = " }, StringSplitOptions.RemoveEmptyEntries);

                if (splitLine[0] == "GROUP" || splitLine[1] == "END_GROUP")
                    continue;

                _metadata[splitLine[0]] = splitLine[1].Trim('"');
            }
        }

        #endregion
    }
}
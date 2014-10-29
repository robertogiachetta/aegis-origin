﻿/// <copyright file="IsodataClustering.cs" company="Eötvös Loránd University (ELTE)">
///     Copyright (c) 2011-2014 Roberto Giachetta. Licensed under the
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
/// <author>Roberto Giachetta</author>

using ELTE.AEGIS.Collections.Segmentation;
using ELTE.AEGIS.Numerics;
using ELTE.AEGIS.Numerics.Statistics;
using ELTE.AEGIS.Operations.Management;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ELTE.AEGIS.Operations.Spectral.Segmentation
{
    /// <summary>
    /// Represents an operation performing clustering of spectral geometries using the ISODATA method.
    /// </summary>
    [OperationMethodImplementation("AEGIS::213701", "ISODATA clustering")]
    public class IsodataClustering : SpectralClustering
    {
        #region Private fields

        /// <summary>
        /// The upper threshold for cluster distance.
        /// </summary>
        private Double _clusterDistanceThreshold;

        /// <summary>
        /// The lower threshold for cluster size.
        /// </summary>
        private Int32 _clusterSizeThreshold;

        /// <summary>
        /// The number of initial cluster centers.
        /// </summary>
        private Int32 _numberOfClusterCenters;

        /// <summary>
        /// The initial cluster centers.
        /// </summary>
        private Double[][] _clusterCenters;
    
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IsodataClustering"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="parameters">The parameters.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The source is null.
        /// or
        /// The method requires parameters which are not specified.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The parameters do not contain a required parameter value.
        /// or
        /// The type of a parameter does not match the type specified by the method.
        /// or
        /// The parameter value does not satisfy the conditions of the parameter.
        /// </exception>
        public IsodataClustering(ISpectralGeometry source, IDictionary<OperationParameter, Object> parameters)
            : this(source, null, parameters)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IsodataClustering"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="parameters">The parameters.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The source is null.
        /// or
        /// The method requires parameters which are not specified.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The parameters do not contain a required parameter value.
        /// or
        /// The type of a parameter does not match the type specified by the method.
        /// or
        /// The value of a parameter is not within the expected range.
        /// or
        /// The specified source and result are the same objects, but the method does not support in-place operations.
        /// or
        /// The source geometry does not contain raster data.
        /// or
        /// The raster format of the source is not supported by the method.
        /// </exception>
        public IsodataClustering(ISpectralGeometry source, SegmentCollection target, IDictionary<OperationParameter, Object> parameters)
            : base(source, target, SpectralOperationMethods.IsodataClustering, parameters)  
        {
            _numberOfClusterCenters = Convert.ToInt32(ResolveParameter(SpectralOperationParameters.NumberOfClusterCenters));
            _clusterDistanceThreshold = Convert.ToInt32(ResolveParameter(SpectralOperationParameters.ClusterDistanceThreshold));
            _clusterSizeThreshold = Convert.ToInt32(ResolveParameter(SpectralOperationParameters.ClusterSizeThreshold));

            if (_numberOfClusterCenters < 10)
                _numberOfClusterCenters = Math.Min(Math.Max(10, Convert.ToInt32(Math.Sqrt(_source.Raster.NumberOfRows * _source.Raster.NumberOfColumns))), _source.Raster.NumberOfRows * _source.Raster.NumberOfColumns);
        }

        #endregion
  
        #region Protected Operation methods

        /// <summary>
        /// Computes the result of the operation.
        /// </summary>
        protected override void ComputeResult()
        {
            CreateInitialClusters();

            // merge spectral values into the clusters
            if (_result.Count < Source.Raster.NumberOfRows * Source.Raster.NumberOfColumns) // in case the initial segments have been provided
                MergeSegmentsToClusters();
            else // if no segments have been provided
                MergeValuesToClusters();

            // eliminate small clusters
            EliminateClusters();

            MergeClusters();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the initial clusters.
        /// </summary>
        private void CreateInitialClusters()
        {            
            // compute median and std. deviation
            Double[] median = new Double[_source.Raster.NumberOfBands];
            Double[] standardDeviation = new Double[_source.Raster.NumberOfBands];

            for (Int32 bandIndex = 0; bandIndex < _source.Raster.NumberOfBands; bandIndex++)
            {
                median[bandIndex] = 0;
                standardDeviation[bandIndex] = 0;

                for (Int32 rowIndex = 0; rowIndex < _source.Raster.NumberOfRows; rowIndex++)
                    for (Int32 columnIndex = 0; columnIndex < _source.Raster.NumberOfColumns; columnIndex++)
                        median[bandIndex] += _source.Raster.GetValue(rowIndex, columnIndex, bandIndex);

                median[bandIndex] /= (_source.Raster.NumberOfColumns * _source.Raster.NumberOfRows);

                for (Int32 rowIndex = 0; rowIndex < _source.Raster.NumberOfRows; rowIndex++)
                    for (Int32 columnIndex = 0; columnIndex < _source.Raster.NumberOfColumns; columnIndex++)
                        standardDeviation[bandIndex] += Calculator.Pow(_source.Raster.GetValue(rowIndex, columnIndex, bandIndex) - median[bandIndex], 2);

                standardDeviation[bandIndex] = Math.Sqrt(standardDeviation[bandIndex] / (_source.Raster.NumberOfRows * _source.Raster.NumberOfColumns));
            }

            // generate the initial clusters
            _clusterCenters = new Double[_numberOfClusterCenters][];
            GaussianRandomGenerator randomGenerator = new GaussianRandomGenerator();

            for (Int32 clusterIndex = 0; clusterIndex < _numberOfClusterCenters; clusterIndex++)
            {
                Double[] randomNumbers = new Double[_source.Raster.NumberOfBands];

                for (Int32 bandIndex = 0; bandIndex < _source.Raster.NumberOfBands; bandIndex++)
                {
                    randomNumbers[bandIndex] = randomGenerator.NextDouble(median[bandIndex], standardDeviation[bandIndex]);
                }

                _clusterCenters[clusterIndex] = randomNumbers;
            }
        }

        /// <summary>
        /// Merges spectral values to clusters.
        /// </summary>
        private void MergeValuesToClusters()
        {
            Segment[] clusters = new Segment[_numberOfClusterCenters];

            Double minimalDistance;
            Int32 minimalIndex = 0;

            for (Int32 rowIndex = 0; rowIndex < _source.Raster.NumberOfRows; rowIndex++)
            {
                for (Int32 columnIndex = 0; columnIndex < _source.Raster.NumberOfColumns; columnIndex++)
                {
                    minimalDistance = Double.MaxValue;

                    for (Int32 clusterIndex = 0; clusterIndex < _clusterCenters.Length; clusterIndex++)
                    {
                        Double distance = 0;
                        switch(Source.Raster.Format)
                        {
                            case RasterFormat.Integer:
                                distance = _distance.Distance(_clusterCenters[clusterIndex], _source.Raster.GetValues(rowIndex, columnIndex));
                                break;
                            case RasterFormat.Floating:
                                distance = _distance.Distance(_clusterCenters[clusterIndex], _source.Raster.GetFloatValues(rowIndex, columnIndex));
                                break;
                        }

                        if (distance < minimalDistance)
                        {
                            minimalDistance = distance;
                            minimalIndex = clusterIndex;
                        }
                    }

                    if (clusters[minimalIndex] == null)
                        clusters[minimalIndex] = _result.GetSegment(rowIndex, columnIndex);
                    else
                        _result.MergeSegments(clusters[minimalIndex], rowIndex, columnIndex);
                }
            }
        }

        /// <summary>
        /// Merges segments to clusters.
        /// </summary>
        private void MergeSegmentsToClusters()
        {
            Segment[] clusters = new Segment[_numberOfClusterCenters];

            Double minimalDistance;
            Int32 minimalIndex = 0;

            foreach (Segment segment in _result.Segments.ToArray())
            {
                if (!_result.Contains(segment))
                    continue;

                minimalDistance = Double.MaxValue;

                for (Int32 clusterIndex = 0; clusterIndex < _clusterCenters.Length; clusterIndex++)
                {
                    Double distance = _distance.Distance(segment, _clusterCenters[clusterIndex]);

                    if (distance < minimalDistance)
                    {
                        minimalDistance = distance;
                        minimalIndex = clusterIndex;
                    }
                }

                if (clusters[minimalIndex] == null)
                    clusters[minimalIndex] = segment;
                else
                    _result.MergeSegments(clusters[minimalIndex], segment);
            }
        }

        private void EliminateClusters()
        {
            _clusterCenters = null;

            Segment[] segments = _result.Segments.ToArray();

            foreach (Segment segment in segments)
            {
                if (segment.Count < _clusterSizeThreshold)
                {
                    _result.SplitSegment(segment);
                }
            }
        }

        /// <summary>
        /// Merges the clusters.
        /// </summary>
        private void MergeClusters()
        {
            Boolean clusterMerged = true;

            while (clusterMerged)
            {
                clusterMerged = false;
                Int32 n = _result.Count;

                List<Segment> segments = _result.Segments.ToList();

                for (Int32 firstIndex = 0; firstIndex < segments.Count - 1; firstIndex++)
                {
                    for (Int32 secondIndex = segments.Count - 1; secondIndex > firstIndex; secondIndex--)
                    {
                        Double distance = _clusterDistance.Distance(segments[firstIndex], segments[secondIndex]);

                        if (Math.Abs(distance) < _clusterDistanceThreshold)
                        {
                            _result.MergeSegments(segments[firstIndex], segments[secondIndex]);
                            segments.RemoveAt(secondIndex);
                            clusterMerged = true;
                        }
                    }
                }
            }
        }

 
        #endregion
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using BitClean;

/*
 * bitclean: Diagnostics.cs
 * Author: Austin Herman
 * 3/11/2019
 */

namespace BitClean
{
	public partial class Diagnostics : Form
	{
		Manager xmlManager = null;

		// list of objects to be displayed in the chart
		List<ChartObject> objectList = new List<ChartObject>();

		// list of radio buttons to choose from
		List<RadioButton> radioHoriztonal;
		List<RadioButton> radioVertical;
		List<RadioButton> radioFunction;

		// structs for object statistics
		AttributeStatistics totalSizeStats		= new AttributeStatistics();
		AttributeStatistics totalDensityStats	= new AttributeStatistics();
		AttributeStatistics totalEdgeratioStats = new AttributeStatistics();
		AttributeStatistics totalNeighborsStats = new AttributeStatistics();

		AttributeStatistics dustSizeStats		= new AttributeStatistics();
		AttributeStatistics dustDensityStats	= new AttributeStatistics();
		AttributeStatistics dustEdgeratioStats	= new AttributeStatistics();
		AttributeStatistics dustNeighborsStats	= new AttributeStatistics();

		AttributeStatistics structureSizeStats		= new AttributeStatistics();
		AttributeStatistics structureDensityStats	= new AttributeStatistics();
		AttributeStatistics structureEdgeratioStats = new AttributeStatistics();
		AttributeStatistics structureNeighborsStats = new AttributeStatistics();

		// for calculating averages
		int dustCount = 0, structureCount = 0;

		// init and set up
		public Diagnostics(Manager xmlManager)
		{
			InitializeComponent();

			// populate horizontal choice radio button list
			radioHoriztonal = new List<RadioButton>() {
				totneighborHorizontal,
				edgeratioHorizontal,
				densityHorizontal,
				avghueHorizontal,
				sizeHorizontal,
				tagHorizontal,
				neuralHorizontal
			};

			// populate vetical choice radio button list
			radioVertical = new List<RadioButton>() {
				totneighborVertical,
				edgeratioVertical,
				densityVertical,
				avghueVertical,
				sizeVertical,
				tagVertical,
				neuralVertical
			};

			// populate function choice radio button list
			radioFunction = new List<RadioButton>() {
				funcNone,
				funcLogistic,
				funcOccur
			};

			// set xml manager
			this.xmlManager = xmlManager;
		}

		private void LoadXML_Click(object sender, EventArgs e)
		{
			if (objectList.Count != 0) // datagrid not empty
				ClearDataGrids();

			using (OpenFileDialog openFD = new OpenFileDialog())
			{
				// File dialog settings
				openFD.Title = "Select an XML file";
				openFD.InitialDirectory = xmlManager.XMLDirectory;
				openFD.Filter = "xml files (*.xml)|*.xml";
				openFD.RestoreDirectory = true;

				// if dialog returned ok, set XML directory and get data from xml sheet
				if (openFD.ShowDialog() == DialogResult.OK) {
					xmlManager.SetXMLDirectory(Path.GetDirectoryName(openFD.FileName));
					GetXMLData(openFD.FileName);
				}

				// if data was loaded, set up data grid
				if(objectList.Count > 0)
					SetUpObjectsDataGrid();
			}

		}

		private void ClearDataGrids()
		{
			// clear all data grids / data lists
			objectList.Clear();

			totalSizeStats.Clear();
			totalDensityStats.Clear();
			totalEdgeratioStats.Clear();
			totalNeighborsStats.Clear();

			dustSizeStats.Clear();
			dustDensityStats.Clear();
			dustEdgeratioStats.Clear();
			dustNeighborsStats.Clear();

			structureSizeStats.Clear();
			structureDensityStats.Clear();
			structureEdgeratioStats.Clear();
			structureNeighborsStats.Clear();

			objectsDataGrid.Rows.Clear();
			totalStatisticsDataGrid.Rows.Clear();
			dustStatisticsDataGrid.Rows.Clear();
			structureStatisticsDataGrid.Rows.Clear();
		}

		private void GetXMLData(string path)
		{
			// load xml doc and set it's root
			XElement doc = XDocument.Load(path).Root;

			// iterate through each object in root
			foreach (var obj in doc.Descendants("object"))
			{
				// create chart object and populate it's data with xml attributes
				ChartObject chartObject = new ChartObject();

				chartObject.tag = (int)obj.Attribute("tag");
				chartObject.decision = (string)obj.Attribute("decision");
				chartObject.size = (int) obj.Element("size");
				chartObject.avghue = (double) obj.Element("avg_hue");
				chartObject.density = (double) obj.Element("density");
				chartObject.edgeratio = (double) obj.Element("edge_ratio");

				chartObject.neighbors = new List<int>();
				foreach (var neighbor in obj.Descendants("tag"))
					chartObject.neighbors.Add((int)neighbor);

				// add chart object to object list
				objectList.Add(chartObject);
			}
		}

		private void SetUpObjectsDataGrid()
		{
			// add chart objects to chart display and get statistics on each chart object
			foreach (ChartObject obj in objectList) {
				objectsDataGrid.Rows.Add(obj.tag, obj.decision, obj.size, obj.avghue, obj.density, obj.edgeratio, obj.neighbors.Count, 0);
				SetUpStatisticsDataGrid(obj);
			}
			// add the data statistics to the stats charts
			AddStatisticsRows();
		}

		private void SetUpStatisticsDataGrid(ChartObject obj)
		{
			// if obj size|density|edgeratio|neighbors is min, set it so
			if (obj.size < totalSizeStats.min || totalSizeStats.min == -1.0)
				totalSizeStats.min = obj.size;
			if (obj.density < totalDensityStats.min || totalDensityStats.min == -1.0)
				totalDensityStats.min = obj.density;
			if (obj.edgeratio < totalEdgeratioStats.min || totalEdgeratioStats.min == -1.0)
				totalEdgeratioStats.min = obj.edgeratio;
			if (obj.neighbors.Count < totalNeighborsStats.min || totalNeighborsStats.min == -1.0)
				totalNeighborsStats.min = obj.neighbors.Count;

			// if obj size|density|edgeratio|neighbors is max, set it so
			if (obj.size > totalSizeStats.max || totalSizeStats.max == -1.0)
				totalSizeStats.max = obj.size;
			if (obj.density > totalDensityStats.max || totalDensityStats.max == -1.0)
				totalDensityStats.max = obj.density;
			if (obj.edgeratio > totalEdgeratioStats.max || totalEdgeratioStats.max == -1.0)
				totalEdgeratioStats.max = obj.edgeratio;
			if (obj.neighbors.Count > totalNeighborsStats.max || totalNeighborsStats.max == -1.0)
				totalNeighborsStats.max = obj.neighbors.Count;

			// add to average
			totalSizeStats.avg += obj.size;
			totalDensityStats.avg += obj.density;
			totalEdgeratioStats.avg += obj.edgeratio;
			totalNeighborsStats.avg += obj.neighbors.Count;

			// if dust, update min/max/avg stats
			if(obj.decision == "dust")
			{
				if (obj.size < dustSizeStats.min || dustSizeStats.min == -1.0)
					dustSizeStats.min = obj.size;
				if (obj.density < dustDensityStats.min || dustDensityStats.min == -1.0)
					dustDensityStats.min = obj.density;
				if (obj.edgeratio < dustEdgeratioStats.min || dustEdgeratioStats.min == -1.0)
					dustEdgeratioStats.min = obj.edgeratio;
				if (obj.neighbors.Count < dustNeighborsStats.min || dustNeighborsStats.min == -1.0)
					dustNeighborsStats.min = obj.neighbors.Count;

				if (obj.size > dustSizeStats.max || dustSizeStats.max == -1.0)
					dustSizeStats.max = obj.size;
				if (obj.density > dustDensityStats.max || dustDensityStats.max == -1.0)
					dustDensityStats.max = obj.density;
				if (obj.edgeratio > dustEdgeratioStats.max || dustEdgeratioStats.max == -1.0)
					dustEdgeratioStats.max = obj.edgeratio;
				if (obj.neighbors.Count > dustNeighborsStats.max || dustNeighborsStats.max == -1.0)
					dustNeighborsStats.max = obj.neighbors.Count;

				dustSizeStats.avg += obj.size;
				dustDensityStats.avg += obj.density;
				dustEdgeratioStats.avg += obj.edgeratio;
				dustNeighborsStats.avg += obj.neighbors.Count;

				dustCount++;
			}
			else
			{ // update structure min/max/avg stats
				if (obj.size < structureSizeStats.min || structureSizeStats.min == -1.0)
					structureSizeStats.min = obj.size;
				if (obj.density < structureDensityStats.min || structureDensityStats.min == -1.0)
					structureDensityStats.min = obj.density;
				if (obj.edgeratio < structureEdgeratioStats.min || structureEdgeratioStats.min == -1.0)
					structureEdgeratioStats.min = obj.edgeratio;
				if (obj.neighbors.Count < structureNeighborsStats.min || structureNeighborsStats.min == -1.0)
					structureNeighborsStats.min = obj.neighbors.Count;

				if (obj.size > structureSizeStats.max || structureSizeStats.max == -1.0)
					structureSizeStats.max = obj.size;
				if (obj.density > structureDensityStats.max || structureDensityStats.max == -1.0)
					structureDensityStats.max = obj.density;
				if (obj.edgeratio > structureEdgeratioStats.max || structureEdgeratioStats.max == -1.0)
					structureEdgeratioStats.max = obj.edgeratio;
				if (obj.neighbors.Count > structureNeighborsStats.max || structureNeighborsStats.max == -1.0)
					structureNeighborsStats.max = obj.neighbors.Count;

				structureSizeStats.avg += obj.size;
				structureDensityStats.avg += obj.density;
				structureEdgeratioStats.avg += obj.edgeratio;
				structureNeighborsStats.avg += obj.neighbors.Count;

				structureCount++;
			}

		}

		private void AddStatisticsRows()
		{
			// add all objects stats
			totalSizeStats.avg /= objectList.Count;
			totalDensityStats.avg /= objectList.Count;
			totalEdgeratioStats.avg /= objectList.Count;
			totalNeighborsStats.avg /= objectList.Count;

			// add dust stats
			dustSizeStats.avg /= dustCount;
			dustDensityStats.avg /= dustCount;
			dustEdgeratioStats.avg /= dustCount;
			dustNeighborsStats.avg /= dustCount;

			// add structure stats
			structureSizeStats.avg /= structureCount;
			structureDensityStats.avg /= structureCount;
			structureEdgeratioStats.avg /= structureCount;
			structureNeighborsStats.avg /= structureCount;

			// create rows
			totalStatisticsDataGrid.Rows.Add("size", totalSizeStats.max, totalSizeStats.min, totalSizeStats.avg, totalSizeStats.mode);
			totalStatisticsDataGrid.Rows.Add("density", totalDensityStats.max, totalDensityStats.min, totalDensityStats.avg, totalDensityStats.mode);
			totalStatisticsDataGrid.Rows.Add("edge ratio", totalEdgeratioStats.max, totalEdgeratioStats.min, totalEdgeratioStats.avg, totalEdgeratioStats.mode);
			totalStatisticsDataGrid.Rows.Add("neighbors", totalNeighborsStats.max, totalNeighborsStats.min, totalNeighborsStats.avg, totalNeighborsStats.mode);

			dustStatisticsDataGrid.Rows.Add("size", dustSizeStats.max, dustSizeStats.min, dustSizeStats.avg, dustSizeStats.mode);
			dustStatisticsDataGrid.Rows.Add("density", dustDensityStats.max, dustDensityStats.min, dustDensityStats.avg, dustDensityStats.mode);
			dustStatisticsDataGrid.Rows.Add("edge ratio", dustEdgeratioStats.max, dustEdgeratioStats.min, dustEdgeratioStats.avg, dustEdgeratioStats.mode);
			dustStatisticsDataGrid.Rows.Add("neighbors", dustNeighborsStats.max, dustNeighborsStats.min, dustNeighborsStats.avg, dustNeighborsStats.mode);

			structureStatisticsDataGrid.Rows.Add("size", structureSizeStats.max, structureSizeStats.min, structureSizeStats.avg, structureSizeStats.mode);
			structureStatisticsDataGrid.Rows.Add("density", structureDensityStats.max, structureDensityStats.min, structureDensityStats.avg, structureDensityStats.mode);
			structureStatisticsDataGrid.Rows.Add("edge ratio", structureEdgeratioStats.max, structureEdgeratioStats.min, structureEdgeratioStats.avg, structureEdgeratioStats.mode);
			structureStatisticsDataGrid.Rows.Add("neighbors", structureNeighborsStats.max, structureNeighborsStats.min, structureNeighborsStats.avg, structureNeighborsStats.mode);
		}

		private void GenerateChart_Click(object sender, EventArgs e)
		{
			int xColIndex = 0, yColIndex = 0;

			// get radio button choice for xAxis (horizontal), yAxis (vertical), function choice
			string horizontalChoice = radioHoriztonal.FirstOrDefault(r => r.Checked).Tag.ToString();
			string verticalChoice	= radioVertical.FirstOrDefault(r => r.Checked).Tag.ToString();
			string functionChoice	= radioFunction.FirstOrDefault(r => r.Checked).Text;

			// relate the radio button choice to the column
			foreach (DataGridViewColumn col in objectsDataGrid.Columns)
			{
				if (col.HeaderText == horizontalChoice)
					xColIndex = col.Index;
				if (col.HeaderText == verticalChoice)
					yColIndex = col.Index;
			}
			
			// create data points to be plotted
			List<object[]> datapoints = new List<object[]>();
			for (int i = 0; i < objectsDataGrid.Rows.Count - 1; i ++)
			{
				object xval, yval, decision, tag;

				if (squareData.Checked && obj_tag.Index != xColIndex) // dont square a tag value
					xval = Math.Pow(Convert.ToDouble(objectsDataGrid.Rows[i].Cells[xColIndex].Value), 2);
				else
					xval = objectsDataGrid.Rows[i].Cells[xColIndex].Value;

				if (squareData.Checked && obj_tag.Index != yColIndex) // dont square a tag value
					yval = Math.Pow(Convert.ToDouble(objectsDataGrid.Rows[i].Cells[yColIndex].Value), 2);
				else
					yval = objectsDataGrid.Rows[i].Cells[yColIndex].Value;


				decision = objectsDataGrid.Rows[i].Cells[obj_decision.Index].Value;
				tag = objectsDataGrid.Rows[i].Cells[obj_tag.Index].Value;

				datapoints.Add(new object[] { xval, yval, decision, tag });
            }

            try {
                // create new chart window and display chart
                ChartDisplay chart = new ChartDisplay(datapoints, horizontalChoice, verticalChoice, displayDust.Checked, displayStructure.Checked, functionChoice);
                chart.Show();
            }
            catch (Exception err) {
                Console.Write(err.Source);
            }
        }

		private void ComputeParameters_Click(object sender, EventArgs e)
		{
			Sifter sifter = new Sifter(new AttributeStatistics[] { dustSizeStats, dustEdgeratioStats, dustDensityStats });

			Logistic logisticFunc = new Logistic();

			sifter.GenerateLogisticParameters(logisticFunc, dustSizeStats);

			SizeParameterLabel.Text = "Size : a = " + Math.Round(logisticFunc.a, 4) + ", b = " + Math.Round(logisticFunc.b, 4) + ", c = " + Math.Round(logisticFunc.c, 4);
		}

		// W.I.P.
		private void Think_Click(object sender, EventArgs e)
		{
			// create brain with 4 attribute inputs
			Brain brain = new Brain(4);

			// for each data object
			for (int i = 0; i < objectsDataGrid.Rows.Count - 1; i++)
			{
				// convert data into inputs, tell brain to "think" and return the outputted network value
				objectsDataGrid.Rows[i].Cells[brain_output.Index].Value = brain.Think(new double[] {
					Convert.ToDouble(objectsDataGrid.Rows[i].Cells[2].Value),
					Convert.ToDouble(objectsDataGrid.Rows[i].Cells[3].Value),
					Convert.ToDouble(objectsDataGrid.Rows[i].Cells[4].Value),
					Convert.ToDouble(objectsDataGrid.Rows[i].Cells[5].Value)
				});
			}
		}
	}
}

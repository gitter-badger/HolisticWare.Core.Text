﻿
// https://github.com/Wintellect/DataScienceExamples/blob/master/ML.NET/MLNetExamples/NewApiExample/Program.cs

using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Trainers;
using MLNetShared;
using System;

namespace NewApiExample
{
    class Program
    {
        static void Main(string[] args)
        {
            string dataset =
                    "SalaryData.csv"
                    //MLNetUtilities.GetDataPathByDatasetName("SalaryData.csv")
                    ;
            string testDataset =
                    "SalaryData-test.csv"
                    //MLNetUtilities.GetDataPathByDatasetName("SalaryData-test.csv")
                    ;

            LocalEnvironment env = new LocalEnvironment();
            Microsoft.ML.StaticPipe.DataReader
                <
                    IMultiStreamSource, 
                    (
                        Microsoft.ML.StaticPipe.Scalar<float> YearsExperience, 
                        Microsoft.ML.StaticPipe.Scalar<float> Target
                    )
                > reader = TextLoader.CreateReader
                                            (
                                                env, 
                                                ctx => 
                                                (
                                                    YearsExperience: ctx.LoadFloat(0),
                                                    Target: ctx.LoadFloat(1)
                                                ), 
                                                hasHeader: true, 
                                                separator: ','
                                            );

            Microsoft.ML.StaticPipe.DataView
                <
                    (
                        Microsoft.ML.StaticPipe.Scalar<float> YearsExperience, 
                        Microsoft.ML.StaticPipe.Scalar<float> Target
                    )
                > data = reader.Read(new MultiFileSource(dataset));

            var regression = new RegressionContext(env);

            var pipeline = reader.MakeNewEstimator()
                .Append(r => (
                    r.Target,
                    Prediction: regression.Trainers.FastTree(label: r.Target, features: r.YearsExperience.AsVector())
                ));

            Microsoft.ML.Core.Data.ITransformer model = pipeline.Fit(data).AsDynamic;

            var predictionFunc = model.MakePredictionFunction<SalaryData, SalaryPrediction>(env);

            var prediction = predictionFunc.Predict(new SalaryData { YearsExperience = 8 });

            Console.WriteLine($"Predicted salary - {String.Format("{0:C}", prediction.PredictedSalary)}");

            Console.Read();
        }
    }
}
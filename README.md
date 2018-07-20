# DataMiningFinal
DataMiningFinal - 代码文件

​	DefaultEntries - 处理各个数据集的数据读取和程序入口

​		Artificial.cs

​		...

​	MalabUtil - 用于计算距离和衡量聚类结果的matlab的dll

​		...

​	DataModel.cs - 数据点类和enum的模型

​	DensityPeak.cs - 单视图的DensityPeak类，继承于View

​	MultiViewDensityPeak.cs - 多视图DensityPeak类

​	View.cs - 视图类，用于保存一个视图的点的数据和一些计算方法

​	Program.cs - 程序的总入口

Datasets - 数据集

​	artificial - 生成的人工数据集，一共340个点，3个视图

​	iris

​	mfeat

​	plant

packages - 包

​	MatFileHandler - 用于读取.mat文件

​	MathNet - 向量表示

​	ValueTuple - Tuple语法糖
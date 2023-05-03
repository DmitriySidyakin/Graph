﻿using GraphAlgorithms.Graph.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphAlgorithms.Graph
{

    public partial class WeightedGraph<DataT>
    {
        #region Fields

        /// <summary>
        /// Шаг пакетизации в массиве узлов по умолчанию
        /// </summary>
        public static long NodeDefaultStep { get; } = 100;

        /// <summary>
        /// Применяемый шаг пакетизации в массиве узлов
        /// </summary>
        public long NodeStep { get; init; }

        /// <summary>
        /// Применяемый шаг пакетизации рёбер графа
        /// </summary>
        public long EdgeStep { get; init; }

        // Далее идут защищаемые поля

        /// <summary>
        /// Массив узлов
        /// </summary>
        internal NodeWithWeightedEdges<DataT>[] Nodes { get; set; }

        /// <summary>
        /// Индекс последнего добавленного узла в поле/массиве Nodes
        /// </summary>
        protected long _LastNodeIndex { get; set; } = -1;

        /// <summary>
        /// Индекс последнего добавленного узла в поле/массиве Nodes
        /// </summary>
        public long LastNodeIndex { get { return _LastNodeIndex; } }

        /// <summary>
        /// Количество узлов
        /// </summary>
        public long NodeCount { get { return _LastNodeIndex + 1; } }

        #endregion

        #region Constructors and their methods

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public WeightedGraph()
        {
            NodeStep = NodeDefaultStep;
            EdgeStep = NodeWithWeightedEdges<DataT>.EdgeDefaultStep;
            Nodes = new NodeWithWeightedEdges<DataT>[NodeStep];
        }

        /// <summary>
        /// Конструктор с заданием шага генерации пакетов сущностей.
        /// </summary>
        /// <param name="nodeStep">Шаг пакетов при наполнении узлов</param>
        /// <param name="edgeStep">Шаг пакетов при наполнении рёбер</param>
        public WeightedGraph(long nodeStep, long edgeStep)
        {
            CheckGraphSteps(nodeStep, edgeStep);

            NodeStep = nodeStep;
            EdgeStep = edgeStep;
            Nodes = new NodeWithWeightedEdges<DataT>[NodeStep];
        }

        /// <summary>
        /// Метод валидирует имеет ли пакетный шаг узлов и рёбер значение больше нуля.
        /// </summary>
        /// <param name="nodeStep">Пакетный шаг узлов</param>
        /// <param name="edgeStep">Пакетный шаг рёбер</param>
        /// <exception cref="GraphException">Исключение генерируется, если пакетный шаг узлов или рёбер меньше 1</exception>
        protected static void CheckGraphSteps(long nodeStep, long edgeStep)
        {
            if (nodeStep <= 0)
                throw new GraphException("Пакетный шаг узлов должен быть больше нуля!", new ArgumentOutOfRangeException(nameof(nodeStep)));
            if (edgeStep <= 0)
                throw new GraphException("Пакетный шаг рёбер должен быть больше нуля!", new ArgumentOutOfRangeException(nameof(edgeStep)));
        }

        #endregion

        #region Initializers

        /// <summary>
        /// Метод добавляет новый узел и возвращает его.
        /// </summary>
        /// <returns>Последний добавленный узел</returns>
        public NodeWithWeightedEdges<DataT> AddEmptyNode()
        {

            if ((_LastNodeIndex + 1) == Nodes.LongLength)
            {
                NodeWithWeightedEdges<DataT>[] oldNodes = Nodes;
                NodeWithWeightedEdges<DataT>[] newNodes = new NodeWithWeightedEdges<DataT>[oldNodes.LongLength + NodeStep];

                for (long i = 0; i < oldNodes.LongLength; i++)
                    newNodes[i] = oldNodes[i];

                Nodes = newNodes;
            }

            _LastNodeIndex++;
            Nodes[_LastNodeIndex] = new NodeWithWeightedEdges<DataT>(_LastNodeIndex, EdgeStep);

            return Nodes[_LastNodeIndex];
        }

        /// <summary>
        /// Метод создаёт связь от узла from до узла to.
        /// </summary>
        /// <param name="from">Начало связи</param>
        /// <param name="to">Конец связи</param>
        /// <returns>Созданная связь</returns>
        public WeightedEdge<DataT>[] AddEdge(NodeWithWeightedEdges<DataT> from, NodeWithWeightedEdges<DataT> to, long weight)
        {
            if (_LastNodeIndex < 1)
                throw
                    new TreeException("Индекс узла не существует!", new ArgumentOutOfRangeException(nameof(to) + " " + nameof(from)));

            if (from.Id > _LastNodeIndex || from.Id < 0)
                throw
                    new GraphException("Индекс узла не существует!", new ArgumentOutOfRangeException(nameof(from)));

            if (to.Id > _LastNodeIndex || to.Id < 0)
                throw
                    new GraphException("Индекс узла не существует!", new ArgumentOutOfRangeException(nameof(to)));

            var edge1 = Nodes[from.Id].AddEdge(Nodes[to.Id], weight);
            var edge2 = Nodes[to.Id].AddEdge(Nodes[from.Id], weight);

            return new[] {edge1, edge2};
        }

        #endregion

        #region Getters

        /// <summary>
        /// Получает узел графа по его индексу.
        /// </summary>
        /// <param name="nodeIndex">Индекс узла</param>
        /// <returns>Узел</returns>
        /// <exception cref="GraphException">Генерируется, если входной индекс не существует в массиве узлов.</exception>
        public NodeWithWeightedEdges<DataT> GetNode(long nodeIndex)
        {
            if (nodeIndex > _LastNodeIndex || nodeIndex < 0)
                throw
                    new GraphException("Индекс узла не существует!", new ArgumentOutOfRangeException(nameof(nodeIndex)));

            return Nodes[nodeIndex];
        }

        /// <summary>
        /// Получает грань определённого узла
        /// </summary>
        /// <param name="nodeIndex">Индекс узла</param>
        /// <param name="edgeIndex">Индекс грани</param>
        /// <returns>Грань с заданными индексами</returns>
        /// <exception cref="GraphException">Генерируется, если входной индекс не существует в массиве узлов или рёбер.</exception>
        public WeightedEdge<DataT> GetEdge(long nodeIndex, long edgeIndex)
        {
            if (nodeIndex > _LastNodeIndex || nodeIndex < 0)
                throw
                    new GraphException("Индекс узла не существует!", new ArgumentOutOfRangeException(nameof(nodeIndex)));

            var node = Nodes[nodeIndex];

            if (edgeIndex > node.LastEdgeIndex || edgeIndex < 0)
                throw
                    new GraphException("Индекс ребра не существует!", new ArgumentOutOfRangeException(nameof(edgeIndex)));

            return node.WeightedEdges[edgeIndex];
        }

        /// <summary>
        /// Метод получает все листья дерева с корнем, т.е. узлы не являющиеся ветвями.
        /// </summary>
        /// <returns>Все листья дерева с корнем, т.е. узлы не являющиеся ветвями.</returns>
        public NodeWithWeightedEdges<DataT>[] GetLeafsAndRoot()
        {
            List<NodeWithWeightedEdges<DataT>> result = new();
            foreach (var n in Nodes)
            {
                if (n != null && n.EdgeCount > 1) { result.Add(n); }
            }

            return result.ToArray();
        }

        #endregion

        #region Graph Properties

        /// <summary>
        /// Свойство показывающее, является ли граф связанным. Возвращает true, если граф связанный, иначе false.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                bool[] explored = new bool[LastNodeIndex + 1];

                if (LastNodeIndex > 0) explored[0] = true;

                for (long nodeIndex = 0; nodeIndex <= LastNodeIndex; nodeIndex++)
                {
                    for (long edgeIndex = 0; edgeIndex <= Nodes[nodeIndex].LastEdgeIndex; edgeIndex++)
                    {
                        explored[Nodes[nodeIndex].WeightedEdges[edgeIndex].To.Id] = true;
                    }
                }

                if (explored.Length > 0)
                {
                    return (from exp in explored
                            where exp == false
                            select exp).Count() == 0;

                }
                else
                {
                    return true;
                }
            }
        }


        #endregion

        #region Different methods

        /// <summary>
        /// Метод возвращает массив, в котором представлен первый индес всеми узлами, второй индекс представлен всеми его исходящими рёбрами.
        /// </summary>
        /// <typeparam name="T">Тип значения по умолчанию, которое присваивается массифицированому элементу графа.</typeparam>
        /// <param name="defualtValue">Значение по умолчанию, которое присваивается массифицированому элементу графа.</param>
        /// <returns>Метод возвращает массив, в котором представлен первый индес всеми узлами, второй индекс представлен всеми его исходящими рёбрами.</returns>
        public T[][] ToEdgeArray<T>(T defualtValue)
        {
            T[][] result = new T[NodeCount][];

            for (var i = 0; i < NodeCount; i++)
            {
                if (Nodes[i].EdgeCount == 0)
                    continue;

                if (result[i] == null)
                    result[i] = new T[Nodes[i].EdgeCount];

                for (var j = 0; j < Nodes[i].EdgeCount; j++)
                {
                    result[i][j] = defualtValue;
                }
            }

            return result;
        }

        #endregion
    }
}

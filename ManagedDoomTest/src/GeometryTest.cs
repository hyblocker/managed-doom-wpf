﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ManagedDoom;

namespace ManagedDoomTest
{
    [TestClass]
    public class GeometryTest
    {
        [TestMethod]
        public void PointOnSide1()
        {
            var random = new Random(666);
            for (var i = 0; i < 1000; i++)
            {
                var startX = -1 - 666 * random.NextDouble();
                var endX = +1 + 666 * random.NextDouble();

                var pointX = 666 * random.NextDouble() - 333;
                var frontSideY = -1 - 666 * random.NextDouble();
                var backSideY = -frontSideY;

                var node = new Node(
                    Fixed.FromDouble(startX),
                    Fixed.Zero,
                    Fixed.FromDouble(endX - startX),
                    Fixed.Zero,
                    Fixed.Zero, Fixed.Zero, Fixed.Zero, Fixed.Zero,
                    Fixed.Zero, Fixed.Zero, Fixed.Zero, Fixed.Zero,
                    0, 0);

                var x = Fixed.FromDouble(pointX);
                {
                    var y = Fixed.FromDouble(frontSideY);
                    Assert.AreEqual(0, Geometry.PointOnSide(x, y, node));
                }
                {
                    var y = Fixed.FromDouble(backSideY);
                    Assert.AreEqual(1, Geometry.PointOnSide(x, y, node));
                }
            }
        }

        [TestMethod]
        public void PointOnSide2()
        {
            var random = new Random(666);
            for (var i = 0; i < 1000; i++)
            {
                var startY = +1 + 666 * random.NextDouble();
                var endY = -1 - 666 * random.NextDouble();

                var pointY = 666 * random.NextDouble() - 333;
                var frontSideX = -1 - 666 * random.NextDouble();
                var backSideX = -frontSideX;

                var node = new Node(
                    Fixed.Zero,
                    Fixed.FromDouble(startY),
                    Fixed.Zero,
                    Fixed.FromDouble(endY - startY),
                    Fixed.Zero, Fixed.Zero, Fixed.Zero, Fixed.Zero,
                    Fixed.Zero, Fixed.Zero, Fixed.Zero, Fixed.Zero,
                    0, 0);

                var y = Fixed.FromDouble(pointY);
                {
                    var x = Fixed.FromDouble(frontSideX);
                    Assert.AreEqual(0, Geometry.PointOnSide(x, y, node));
                }
                {
                    var x = Fixed.FromDouble(backSideX);
                    Assert.AreEqual(1, Geometry.PointOnSide(x, y, node));
                }
            }
        }

        [TestMethod]
        public void PointOnSide3()
        {
            var random = new Random(666);
            for (var i = 0; i < 1000; i++)
            {
                var startX = -1 - 666 * random.NextDouble();
                var endX = +1 + 666 * random.NextDouble();

                var pointX = 666 * random.NextDouble() - 333;
                var frontSideY = -1 - 666 * random.NextDouble();
                var backSideY = -frontSideY;

                for (var j = 0; j < 100; j++)
                {
                    var theta = 2 * Math.PI * random.NextDouble();
                    var ox = 666 * random.NextDouble() - 333;
                    var oy = 666 * random.NextDouble() - 333;

                    var node = new Node(
                        Fixed.FromDouble(ox + startX * Math.Cos(theta)),
                        Fixed.FromDouble(oy + startX * Math.Sin(theta)),
                        Fixed.FromDouble((endX - startX) * Math.Cos(theta)),
                        Fixed.FromDouble((endX - startX) * Math.Sin(theta)),
                        Fixed.Zero, Fixed.Zero, Fixed.Zero, Fixed.Zero,
                        Fixed.Zero, Fixed.Zero, Fixed.Zero, Fixed.Zero,
                        0, 0);

                    {
                        var x = Fixed.FromDouble(ox + pointX * Math.Cos(theta) - frontSideY * Math.Sin(theta));
                        var y = Fixed.FromDouble(oy + pointX * Math.Sin(theta) + frontSideY * Math.Cos(theta));
                        Assert.AreEqual(0, Geometry.PointOnSide(x, y, node));
                    }
                    {
                        var x = Fixed.FromDouble(ox + pointX * Math.Cos(theta) - backSideY * Math.Sin(theta));
                        var y = Fixed.FromDouble(oy + pointX * Math.Sin(theta) + backSideY * Math.Cos(theta));
                        Assert.AreEqual(1, Geometry.PointOnSide(x, y, node));
                    }
                }
            }
        }

        [TestMethod]
        public void PointToDist()
        {
            var random = new Random(666);
            for (var i = 0; i < 1000; i += 3)
            {
                var expected = i;
                for (var j = 0; j < 100; j++)
                {
                    var r = i;
                    var theta = 2 * Math.PI * random.NextDouble();
                    var ox = 666 * random.NextDouble() - 333;
                    var oy = 666 * random.NextDouble() - 333;
                    var x = ox + r * Math.Cos(theta);
                    var y = oy + r * Math.Sin(theta);
                    var fromX = Fixed.FromDouble(ox);
                    var fromY = Fixed.FromDouble(oy);
                    var toX = Fixed.FromDouble(x);
                    var toY = Fixed.FromDouble(y);
                    var dist = Geometry.PointToDist(fromX, fromY, toX, toY);
                    Assert.AreEqual(expected, dist.ToDouble(), (double)i / 100);
                }
            }
        }

        [TestMethod]
        public void PointToAngle()
        {
            var random = new Random(666);
            for (var i = 0; i < 100; i++)
            {
                var expected = 2 * Math.PI * random.NextDouble();
                for (var j = 0; j < 100; j++)
                {
                    var r = 666 * random.NextDouble();
                    var ox = 666 * random.NextDouble() - 333;
                    var oy = 666 * random.NextDouble() - 333;
                    var x = ox + r * Math.Cos(expected);
                    var y = oy + r * Math.Sin(expected);
                    var fromX = Fixed.FromDouble(ox);
                    var fromY = Fixed.FromDouble(oy);
                    var toX = Fixed.FromDouble(x);
                    var toY = Fixed.FromDouble(y);
                    var angle = Geometry.PointToAngle(fromX, fromY, toX, toY);
                    var actual = angle.ToRadian();
                    Assert.AreEqual(expected, actual, 0.01);
                }
            }
        }
    }
}
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;

public class BlueprintTest
{

    [Test]
    public void TestDiscrete3DCoordHashAndEquals()
    {
        Discrete3DCoord a = new Discrete3DCoord(1, 2, 3);
        Discrete3DCoord a_copy = new Discrete3DCoord(1, 2, 3);
        Discrete3DCoord b = new Discrete3DCoord(2, 3, 4);
        Assert.AreEqual(a, a_copy);
        Assert.AreEqual(a.GetHashCode(), a_copy.GetHashCode());
        Assert.AreNotEqual(a, b);
        Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [Test]
    public void TestConstructor()
    {
        Assert.Throws<System.ArgumentException>(() => new Blueprint(-1, 2, 3));
        Assert.Throws<System.ArgumentException>(() => new Blueprint(1, -2, 3));
        Assert.Throws<System.ArgumentException>(() => new Blueprint(1, 2, -3));

        new Blueprint(1, 2, 3);
        new Blueprint(new Discrete3DCoord(1, 2, 3));
    }

    [Test]
    public void TestGetDims()
    {
        Blueprint a = new Blueprint(1, 2, 3);
        Discrete3DCoord dims = a.GetDims();
        Assert.AreEqual(1, dims.x);
        Assert.AreEqual(2, dims.y);
        Assert.AreEqual(3, dims.z);

        Blueprint a_copy = new Blueprint(new Discrete3DCoord(1, 2, 3));

        Assert.AreEqual(dims, a_copy.GetDims());
    }

    [Test]
    public void TestGetBlocks()
    {
        Blueprint a = new Blueprint(5, 7, 1);
        int[,,] a_blocks = a.GetBlocks();
        Assert.AreEqual(5, a_blocks.GetLength(0));
        Assert.AreEqual(7, a_blocks.GetLength(1));
        Assert.AreEqual(1, a_blocks.GetLength(2));

        foreach(int i in a_blocks)
        {
            Assert.AreEqual(i, 0);
        }
    }

    [Test]
    public void TestAddBlock()
    {
        Blueprint a = new Blueprint(3, 3, 3);
        a.AddBlock(new Discrete3DCoord(0, 0, 0), 1);
        Assert.AreEqual(1, a.GetBlocks()[0, 0, 0]);
        a.AddBlock(new Discrete3DCoord(1, 2, 0), 3);
        Assert.AreEqual(1, a.GetBlocks()[1, 2, 0], 3);
    }

    [Test]
    public void TestCopyInto()
    {
        Blueprint a = new Blueprint(3, 3, 3);
        a.AddBlock(new Discrete3DCoord(0, 0, 0), 1);
        a.AddBlock(new Discrete3DCoord(0, 0, 1), 1);
        a.AddBlock(new Discrete3DCoord(0, 0, 2), 1);

        Blueprint mismatched_size = new Blueprint(2, 3, 3);
        Assert.Throws<System.ArgumentException>(() => a.CopyInto(mismatched_size));

        Blueprint copy = new Blueprint(3, 3, 3);
        a.CopyInto(copy);
        CollectionAssert.AreEqual(a.GetBlocks(), copy.GetBlocks());
    }

    [Test]
    public void TestRotate()
    {
        Blueprint x_line = new Blueprint(3, 3, 3);
        x_line.AddBlock(new Discrete3DCoord(0, 0, 0), 1);
        x_line.AddBlock(new Discrete3DCoord(1, 0, 0), 1);
        x_line.AddBlock(new Discrete3DCoord(2, 0, 0), 1);

        Blueprint y_line = x_line.Rotate(RotationAxis.Z_AXIS, 1);
        Blueprint z_line = x_line.Rotate(RotationAxis.Y_AXIS, 1);

        Assert.AreEqual(1, y_line.GetBlocks()[2, 0, 0]);
        Assert.AreEqual(1, y_line.GetBlocks()[2, 1, 0]);
        Assert.AreEqual(1, y_line.GetBlocks()[2, 2, 0]);

        Assert.AreEqual(1, z_line.GetBlocks()[2, 0, 0]);
        Assert.AreEqual(1, z_line.GetBlocks()[2, 0, 1]);
        Assert.AreEqual(1, z_line.GetBlocks()[2, 0, 2]);

        CollectionAssert.AreEqual(
            x_line.GetBlocks(), 
            y_line.Rotate(RotationAxis.Z_AXIS, 3).GetBlocks());

        CollectionAssert.AreEqual(
            x_line.GetBlocks(),
            z_line.Rotate(RotationAxis.Y_AXIS, -1).GetBlocks());
    }

    [Test]
    public void TestIsStable()
    {
        Blueprint stable = new Blueprint(3, 3, 3);
        Assert.IsTrue(stable.IsStable());
        stable.AddBlock(new Discrete3DCoord(0, 0, 0), 1);
        Assert.IsTrue(stable.IsStable());
        stable.AddBlock(new Discrete3DCoord(0, 1, 0), 1);
        stable.AddBlock(new Discrete3DCoord(2, 0, 0), 2);
        stable.AddBlock(new Discrete3DCoord(2, 1, 0), 2);
        Assert.IsTrue(stable.IsStable());
        stable.AddBlock(new Discrete3DCoord(0, 2, 0), 3);
        stable.AddBlock(new Discrete3DCoord(1, 2, 0), 3);
        stable.AddBlock(new Discrete3DCoord(2, 2, 0), 3);
        Assert.IsTrue(stable.IsStable());
        Blueprint unstable = new Blueprint(3, 3, 3);
        unstable.AddBlock(new Discrete3DCoord(1, 1, 1), 1);
        Assert.IsFalse(unstable.IsStable());
    }

    [Test]
    public void TestDeleteID()
    {
        Blueprint empty = new Blueprint(4, 4, 4);
        Blueprint arch = new Blueprint(4, 4, 4);

        CollectionAssert.AreEqual(empty.GetBlocks(), arch.GetBlocks());

        arch.AddBlock(new Discrete3DCoord(0, 0, 0), 1);
        arch.AddBlock(new Discrete3DCoord(0, 1, 0), 1);
        arch.AddBlock(new Discrete3DCoord(0, 2, 0), 1);

        arch.AddBlock(new Discrete3DCoord(2, 0, 0), 1);
        arch.AddBlock(new Discrete3DCoord(2, 1, 0), 1);
        arch.AddBlock(new Discrete3DCoord(2, 2, 0), 1);

        arch.AddBlock(new Discrete3DCoord(0, 3, 0), 1);
        arch.AddBlock(new Discrete3DCoord(1, 3, 0), 1);
        arch.AddBlock(new Discrete3DCoord(2, 3, 0), 1);

        CollectionAssert.AreNotEqual(empty.GetBlocks(), arch.GetBlocks());

        arch.DeleteID(1);
        CollectionAssert.AreEqual(empty.GetBlocks(), arch.GetBlocks());
    }

    [Test]
    public void TestApplyDesign()
    {
        Blueprint composite = new Blueprint(4, 5, 4);

        Blueprint long_block = new Blueprint(4, 1, 1);
        long_block.AddBlock(new Discrete3DCoord(0, 0, 0), 1);
        long_block.AddBlock(new Discrete3DCoord(1, 0, 0), 1);
        long_block.AddBlock(new Discrete3DCoord(2, 0, 0), 1);
        long_block.AddBlock(new Discrete3DCoord(3, 0, 0), 1);

        Blueprint tall_block = new Blueprint(2, 3, 1);
        tall_block.AddBlock(new Discrete3DCoord(0, 0, 0), 1);
        tall_block.AddBlock(new Discrete3DCoord(0, 1, 0), 1);
        tall_block.AddBlock(new Discrete3DCoord(0, 2, 0), 1);

        Blueprint really_really_long_block = new Blueprint(10, 1, 1);
        really_really_long_block.AddBlock(new Discrete3DCoord(0, 0, 0), 1);
        really_really_long_block.AddBlock(new Discrete3DCoord(1, 0, 0), 1);
        really_really_long_block.AddBlock(new Discrete3DCoord(2, 0, 0), 1);
        really_really_long_block.AddBlock(new Discrete3DCoord(3, 0, 0), 1);
        really_really_long_block.AddBlock(new Discrete3DCoord(4, 0, 0), 1);
        really_really_long_block.AddBlock(new Discrete3DCoord(5, 0, 0), 1);
        really_really_long_block.AddBlock(new Discrete3DCoord(6, 0, 0), 1);
        really_really_long_block.AddBlock(new Discrete3DCoord(7, 0, 0), 1);
        really_really_long_block.AddBlock(new Discrete3DCoord(8, 0, 0), 1);
        really_really_long_block.AddBlock(new Discrete3DCoord(9, 0, 0), 1);

        composite.ApplyDesign(long_block, 0, 0, 0);

        List<int> unique_ids = new List<int>();

        unique_ids.Add(composite.GetBlocks()[0,0,0]);
        CollectionAssert.AllItemsAreUnique(unique_ids);
        CollectionAssert.DoesNotContain(unique_ids, 0);

        composite.ApplyDesign(tall_block, 0, 1, 0);
        unique_ids.Add(composite.GetBlocks()[0, 1, 0]);
        CollectionAssert.AllItemsAreUnique(unique_ids);
        CollectionAssert.DoesNotContain(unique_ids, 0);

        composite.ApplyDesign(tall_block, 3, 1, 0);
        unique_ids.Add(composite.GetBlocks()[3, 1, 0]);
        CollectionAssert.AllItemsAreUnique(unique_ids);
        CollectionAssert.DoesNotContain(unique_ids, 0);

        composite.ApplyDesign(really_really_long_block, 0, 3, 0);
        unique_ids.Add(composite.GetBlocks()[0, 3, 0]);
        CollectionAssert.AllItemsAreUnique(unique_ids);
        CollectionAssert.DoesNotContain(unique_ids, 0);

        Assert.AreEqual(composite.GetBlocks()[0, 3, 0], composite.GetBlocks()[3, 3, 0]);
    }

    [Test]
    public void TestInstantiate()
    {

    }

    [Test]
    public void TestSaveAsPrefab()
    {

    }

    [Test]
    public void TestMutate()
    {

    }
}

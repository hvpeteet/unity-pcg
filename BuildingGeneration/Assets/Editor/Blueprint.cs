using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Discrete3DCoord
{
    public readonly int x;
    public readonly int y;
    public readonly int z;

    public Discrete3DCoord(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override bool Equals(object obj)
    {
        Discrete3DCoord other = obj as Discrete3DCoord;

        return other.x == this.x && other.y == this.y && other.z == this.z;
    }

    public override int GetHashCode()
    {
        return this.x << 10 | this.y << 10 | this.z << 10;
    }
}

delegate Discrete3DCoord TranformationFunc(Discrete3DCoord point);

public enum RotationAxis
{
    X_AXIS,
    Y_AXIS,
    Z_AXIS
}

public class Blueprint {
    private const int MAX_MUTATION_ATTEMPTS = 3;
    private const int NUM_INIT_MUTATIONS = 10;
    private const double DELETE_CHANCE = 0.1;
    private const int MAX_PLACEMENT_ATTEMPTS = 3;

    private static List<Blueprint> design_notebook = InitDesignNotebook();

    private int[,,] blocks;
    private int next_subdesign_id = 0;
    private Discrete3DCoord dims;

    private HashSet<int> valid_ids = new HashSet<int>();
    private HashSet<Discrete3DCoord> attatchment_points = new HashSet<Discrete3DCoord>();

    // -------------- Basic Methods --------------
    public Blueprint(int dim_x, int dim_y, int dim_z)
    {
        blocks = new int[dim_x, dim_y, dim_z];
        this.dims = new Discrete3DCoord(dim_x, dim_y, dim_z);
        for (int x = 0; x < dim_x; x++)
        {
            for (int z = 0; z < dim_z; z++)
            {
                this.attatchment_points.Add(new Discrete3DCoord(x, 0, z));
            }
        }
    }

    public Blueprint(Discrete3DCoord dims) : this(dims.x, dims.y, dims.z) { }

    public int[,,] GetBlocks()
    {
        return (int[,,])this.blocks.Clone();
    }

    public void Clear()
    {
        int[] my_dims = this.GetDimsArr();
        // Zero out all the blocks
        for (int x = 0; x < my_dims[0]; x++)
        {
            for (int y = 0; y < my_dims[1]; y++)
            {
                for (int z = 0; z < my_dims[2]; z++)
                {
                    this.blocks[z, y, z] = 0;
                }
            }
        }
    }

    public int[] GetDimsArr()
    {
        int[] dims = new int[] { this.dims.x, this.dims.y, this.dims.z };
        return dims;
    }

    public Discrete3DCoord GetDims()
    {
        return this.dims;
    }

    public string GetDimsString()
    {
        return string.Format("{0} x {1} x {2}", blocks.GetLength(0), blocks.GetLength(1), blocks.GetLength(2));
    }

    public void CopyInto(Blueprint target)
    {
        int[] my_dims = this.GetDimsArr();
        // Check that the dimensions are the same
        if (!target.GetDimsArr().SequenceEqual(my_dims))
        {
            Debug.Log(string.Format(
                "Blueprint dimensions must be exactly the same, were {0} (source) and {1} (target)",
                this.GetDimsString(),
                target.GetDimsString()));
        }
        // Copy this into target
        System.Array.Copy(this.blocks, target.blocks, this.blocks.Length);
        target.next_subdesign_id = this.next_subdesign_id;
        target.dims = this.dims;
        target.valid_ids = new HashSet<int>(this.valid_ids);
        target.attatchment_points = new HashSet<Discrete3DCoord>(this.attatchment_points);
}

    public void AddDesignToNotebook(Blueprint new_design)
    {
        design_notebook.Add(new_design);
    }

    public Blueprint Rotate(RotationAxis axis, int rotation_magnitude)
    {
        int[] my_dims = this.GetDimsArr();
        Blueprint rotated = new Blueprint(my_dims[0], my_dims[1], my_dims[2]);
        this.CopyInto(rotated);
        for (int i = 0; i < rotation_magnitude % 4; i++)
        {
            rotated._Rotate(axis);
        }
        return rotated;
    }

    public void AddBlock(Discrete3DCoord coord, int subdesign_id)
    {
        this.blocks[coord.x, coord.y, coord.z] = subdesign_id;
        if (!valid_ids.Contains(subdesign_id))
        {
            valid_ids.Add(subdesign_id);
        }
        if (next_subdesign_id <= subdesign_id)
        {
            next_subdesign_id = subdesign_id + 1;
        }
    }

    // -------------- Genetic functions --------------

    public GameObject Instantiate()
    {
        System.Random rng = new System.Random();
        int[] my_dims = this.GetDimsArr();
        GameObject base_cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        base_cube.AddComponent(typeof(Rigidbody));

        Dictionary<int, GameObject> subdesign_core_objs = new Dictionary<int, GameObject>();

        List<GameObject> physical_blocks = new List<GameObject>();
        // populate the materials list
        List<Material> materials = new List<Material>();
        string[] material_asset_guids = UnityEditor.AssetDatabase.FindAssets("t:material", new string[] { "Assets/Prefabs/Ruins/Materials" });
        for (int i = 0; i < this.next_subdesign_id; i++)
        {
            string chosen_material_guid = material_asset_guids[rng.Next(material_asset_guids.Length)];
            Material material = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(UnityEditor.AssetDatabase.GUIDToAssetPath(chosen_material_guid));
            materials.Add(material);
        }
        for (int i = 0; i < my_dims[0]; i++)
        {
            for (int j = 0; j < my_dims[1]; j++)
            {
                for (int k = 0; k < my_dims[2]; k++)
                {
                    int subdesign = this.blocks[i, j, k];
                    if (subdesign != 0)
                    {
                        GameObject obj = Object.Instantiate(
                            base_cube,
                            new Vector3(i + 0.5f, j + 0.5f, k),
                            new Quaternion());
                        physical_blocks.Add(obj);
                        
                        obj.GetComponent<Renderer>().material = materials[subdesign];
                        if (!subdesign_core_objs.ContainsKey(subdesign))
                        {
                            subdesign_core_objs.Add(subdesign, obj);
                        }
                        else
                        {
                            FixedJoint joint = subdesign_core_objs[subdesign].AddComponent<FixedJoint>();
                            joint.connectedBody = obj.GetComponent<Rigidbody>();
                        }
                    }
                }
            }
        }
        Object.DestroyImmediate(base_cube);

        GameObject container = new GameObject();
        foreach (GameObject g in physical_blocks)
        {
            g.transform.parent = container.transform;
        }

        return container;
    }

    public Blueprint Randomize()
    {
        Blueprint randomized = new Blueprint(
            blocks.GetLength(0),
            blocks.GetLength(1),
            blocks.GetLength(2));
        for (int i = 0; i < NUM_INIT_MUTATIONS; i++)
        {
            randomized = randomized.Mutate();
        }
        return randomized;
    }

    public Blueprint Mutate()
    {
        Blueprint mutant = new Blueprint(
            blocks.GetLength(0),
            blocks.GetLength(1),
            blocks.GetLength(2));
        int i = 0;
        do
        {
            this.CopyInto(mutant);
            if (i >= MAX_MUTATION_ATTEMPTS)
            {
                Debug.Log("Could not find a valid mutation, returning the base object");
                break;
            }
            mutant._UnstableMutate();
            i++;
        } while (!mutant.IsStable());
        return mutant;
    }

    public void MutateInto(Blueprint target)
    {
        this.Mutate().CopyInto(target);
    }

    public void RandomizeInto(Blueprint target)
    {
        this.Randomize().CopyInto(target);
    }

    // -------------- Private functions --------------

    private static List<Blueprint> InitDesignNotebook()
    {
        List<Blueprint> notebook = new List<Blueprint>();

        // Single block
        Blueprint starting_block = new Blueprint(1, 1, 1);
        starting_block.AddBlock(new Discrete3DCoord(0, 0, 0), 1);
        notebook.Add(starting_block);

        // 2x1 brick
        Blueprint brick2 = new Blueprint(2, 1, 1);
        brick2.AddBlock(new Discrete3DCoord(0, 0, 0), 1);
        brick2.AddBlock(new Discrete3DCoord(1, 0, 0), 1);
        notebook.Add(brick2);

        // 3x1 brick
        Blueprint brick3 = new Blueprint(3, 1, 1);
        brick3.AddBlock(new Discrete3DCoord(0, 0, 0), 1);
        brick3.AddBlock(new Discrete3DCoord(1, 0, 0), 1);
        brick3.AddBlock(new Discrete3DCoord(2, 0, 0), 1);
        notebook.Add(brick3);

        return notebook;
    }

    private bool IsStable()
    {
        GameObject building = this.Instantiate();

        List<Vector3> orig_pos = new List<Vector3>();
        foreach (Transform obj in building.GetComponentsInChildren<Transform>())
        {
            orig_pos.Add(obj.transform.position);
        }
        this.SimulatePhysics(10.0f);

        int i = 0;
        bool stable = true;
        foreach (Transform obj in building.GetComponentsInChildren<Transform>())
        {
            if ((obj.transform.position - orig_pos[i]).magnitude > 0.1f)
            {
                stable = false;
            }
            i++;
        }
        Object.DestroyImmediate(building);
        return stable;
    }

    private void SimulatePhysics(float seconds)
    {
        bool auto_was_on = Physics.autoSimulation;
        Physics.autoSimulation = false;

        float timer = Time.deltaTime + seconds;

        // Catch up with the game time.
        // Advance the physics simulation in portions of Time.fixedDeltaTime
        // Note that generally, we don't want to pass variable delta to Simulate as that leads to unstable results.
        while (timer >= Time.fixedDeltaTime)
        {
            timer -= Time.fixedDeltaTime;
            Physics.Simulate(Time.fixedDeltaTime);
        }

        Physics.autoSimulation = auto_was_on;
    }

    private void _UnstableMutate()
    {

        System.Random rng = new System.Random();
        // 0) Decide if we are adding or deleting.
        if (valid_ids.Count() == 0 || rng.NextDouble() > DELETE_CHANCE)
        {
            bool placed_block = false;
            int tries = 0;
            while (tries < MAX_PLACEMENT_ATTEMPTS && !placed_block)
            {
                // ------------- ADDING -------------
                // 1) Pick a design from the notebook randomly
                int i = rng.Next(design_notebook.Count);
                // Debug.Log(string.Format("Using design {0}", i));
                Blueprint random_design = design_notebook[i];
                // 2) Randomly rotate the object
                System.Array enum_values = System.Enum.GetValues(typeof(RotationAxis));
                RotationAxis axis = (RotationAxis)enum_values.GetValue(rng.Next(enum_values.Length));
                int mag = rng.Next(3);
                Blueprint rotated = random_design.Rotate(axis, mag);
                // 3) Try to find a place for it
                Discrete3DCoord placement = FindValidOffsetFor(rotated);
                if (placement != null)
                {
                    this.ApplyDesign(rotated, placement.x, placement.y, placement.z);
                    placed_block = true;
                }
                // 4) Upon failure try again, limited to n tries
                tries++;
            }
            if (tries == MAX_PLACEMENT_ATTEMPTS && !placed_block)
            {
                // TODO: Figure out how to handle this error nicely
                Debug.Log("Could not find a valid mutation");
            }
        }
        else
        {
            // ------------- DELETING-------------
            // TODO: Keep track of a set of valid ids
            // 1) Pick a valid id
            int[] valid_arr = valid_ids.ToArray();
            int random_id = valid_arr[rng.Next(valid_arr.Count())];
            // 2) Zero out all blocks with the same id
            // 3) Invalidate the id
            this.DeleteID(random_id);
        }
    }

    // Returns null if it can't find a valid placement for the blueprint
    private Discrete3DCoord FindValidOffsetFor(Blueprint b)
    {
        System.Random rng = new System.Random();
        List<Discrete3DCoord> valid_coords = new List<Discrete3DCoord>();
        foreach (Discrete3DCoord coord in attatchment_points)
        {
            // TODO: Maybe try to ensure that the new block's position has at least 1 supporting block.
            if (!this.DesignCollides(b, coord.x, coord.y, coord.z))
            {
                valid_coords.Add(coord);
            }
        }
        if (valid_coords.Count > 0)
        {
            return valid_coords[rng.Next(valid_coords.Count)];
        }
        return null;
    }

    // Check if a design collides with the current layout
    private bool DesignCollides(Blueprint design, int x_start, int y_start, int z_start)
    {
        int[] my_dims = this.GetDimsArr();
        int[] other_dims = design.GetDimsArr();
        int[] copy_dims = {
            Mathf.Min(my_dims[0] - x_start, other_dims[0]),
            Mathf.Min(my_dims[1] - y_start, other_dims[1]),
            Mathf.Min(my_dims[2] - z_start, other_dims[2])
        };
        for (int x = 0; x < copy_dims[0]; x++)
        {
            for (int y = 0; y < copy_dims[1]; y++)
            {
                for (int z = 0; z < copy_dims[2]; z++)
                {
                    if (design.blocks[x, y, z] > 0 && this.blocks[x + x_start, y + y_start, z + z_start] > 0)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void DeleteID(int id)
    {
        for (int x = 0; x < this.dims.x; x++)
        {
            for (int y = 0; y < this.dims.y; y++)
            {
                for (int z = 0; z < this.dims.z; z++)
                {
                    if (this.blocks[x, y, z] == id)
                    {
                        this.blocks[x, y, z] = 0;
                        // Add an attatchment point if this is on the floor or has a block below it.
                        if (y == 0 || (this.blocks[x, y - 1, z] != 0 && this.blocks[x, y - 1, z] != id))
                        {
                            attatchment_points.Add(new Discrete3DCoord(x, y, z));
                        }
                    }
                }
            }
        }
        this.valid_ids.Remove(id);
    }

    // Copy over a blueprint at location (x,y,z).
    // NOTE: If the blueprint would overflow any of the dimensions 
    //       the operation will truncate the design in order to fit.
    private void ApplyDesign(Blueprint design, int x_start, int y_start, int z_start)
    {
        int subdesign_id = this.next_subdesign_id;
        next_subdesign_id += design.next_subdesign_id;
        int[] my_dims = this.GetDimsArr();
        int[] other_dims = design.GetDimsArr();
        int[] copy_dims = {
            Mathf.Min(my_dims[0] - x_start, other_dims[0]),
            Mathf.Min(my_dims[1] - y_start, other_dims[1]),
            Mathf.Min(my_dims[2] - z_start, other_dims[2])
        };
        for (int x = 0; x < copy_dims[0]; x++)
        {
            for (int y = 0; y < copy_dims[1]; y++)
            {
                for (int z = 0; z < copy_dims[2]; z++)
                {
                    if (design.blocks[x, y, z] > 0)
                    {
                        int this_x = x + x_start;
                        int this_y = y + y_start;
                        int this_z = z + z_start;
                        this.blocks[this_x, this_y, this_z] = subdesign_id + design.blocks[x, y, z];
                        if (this_y + 1 < this.dims.y && this.blocks[this_x, this_y + 1, this_z] == 0)
                        {
                            attatchment_points.Add(new Discrete3DCoord(this_x, this_y + 1, this_z));
                        }
                        Discrete3DCoord this_coord = new Discrete3DCoord(this_x, this_y, this_z);
                        if (attatchment_points.Contains(this_coord))
                        {
                            attatchment_points.Remove(this_coord);
                        }
                    }
                }
            }
        }
    }

    private void _ApplyTransformation(TranformationFunc f, Discrete3DCoord new_dims)
    {
        int[,,] new_blocks = new int[new_dims.x, new_dims.y, new_dims.z];
        for (int x = 0; x < this.dims.x; x++)
        {
            for (int y = 0; y < this.dims.y; y++)
            {
                for (int z = 0; z < this.dims.z; z++)
                {
                    Discrete3DCoord new_coord = f(new Discrete3DCoord(x, y, z));
                    new_blocks[new_coord.x, new_coord.y, new_coord.z] = this.blocks[x, y, z];
                }
            }
        }
        this.blocks = new_blocks;
        this.dims = new_dims;
    }

    // Rotates this object around the desired axis in a counter clockwise direction 
    // (viewed from the + end of the axis)
    private void _Rotate(RotationAxis axis)
    {
        TranformationFunc f;
        Discrete3DCoord new_dims;
        switch (axis) {
            case RotationAxis.X_AXIS:
                new_dims = new Discrete3DCoord(this.dims.x, this.dims.z, this.dims.y);
                f = coord => new Discrete3DCoord(coord.x, coord.z, this.dims.y - coord.y - 1);
                break;
            case RotationAxis.Y_AXIS:
                new_dims = new Discrete3DCoord(this.dims.z, this.dims.y, this.dims.x);
                f = coord => new Discrete3DCoord(this.dims.z - coord.z - 1, coord.y, coord.x);
                break;
            case RotationAxis.Z_AXIS:
                new_dims = new Discrete3DCoord(this.dims.y, this.dims.x, this.dims.z);
                f = coord => new Discrete3DCoord(this.dims.y - coord.y - 1, coord.x, coord.z);
                break;
            default:
                throw new System.Exception(string.Format("Attempted to rotate along axis {0} which is not accounted for", axis));
        }
        _ApplyTransformation(f, new_dims);
    }
}

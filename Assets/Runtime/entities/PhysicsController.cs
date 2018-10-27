﻿using UnityEngine;

public sealed class PhysicsController : RayCollider {

  [ReadOnly]
  public Vector2 velocity;

  [Header("Layer Masks")]
  [SerializeField]
  private LayerMask groundMask = 0;
  [SerializeField]
  private LayerMask platformMask = 0;

  private bool ignorePlatforms = false;
  // Prevent heap allocations
  private readonly RaycastHit2D[] hit = new RaycastHit2D[1];
  private Vector2 ray;

  public struct CollisionState {
    public bool Left { get; set; }
    public bool Right { get; set; }
    public bool Above { get; set; }
    public bool Below { get; set; }

    public bool Collision {
      get { return Left || Right || Above || Below; }
    }

    public void Reset() {
      this.Left = false;
      this.Right = false;
      this.Above = false;
      this.Below = false;
    }
  }
  private CollisionState state;

  #region Properties
  public CollisionState Collision { get { return this.state; } }
  public bool Grounded { get { return this.state.Below; } }
  public bool IgnorePlatforms {
    get { return this.ignorePlatforms; }
    set { this.ignorePlatforms = value; }
  }

  public LayerMask Ground {
    get { return this.groundMask; }
  }
  #endregion

  public void Move(Vector2 movement) {
    this.UpdateRayOrigins();
    this.state.Reset();

    if (movement.x != 0f) {
      this.CalculateHorizontal(ref movement);
    }
    if (movement.y != 0f) {
      this.CalculateVertical(ref movement);
    }

    this.transform.Translate(movement, Space.World);

    if (Time.deltaTime > 0f) {
      this.velocity = movement / Time.deltaTime;
    }
    this.ignorePlatforms = false;
  }

  private void CalculateHorizontal(ref Vector2 movement) {
    bool right = movement.x > 0;
    float distance = Mathf.Abs(movement.x) + this.Inset;
    Vector2 direction, origin;
    if (right) {
      direction = Vector2.right;
      origin = this.Origins.bottomRight;
    } else {
      direction = Vector2.left;
      origin = this.Origins.bottomLeft;
    }

    for (int i = 0; i < this.HorizontalRays; i++) {
      this.ray = new Vector2(origin.x, origin.y + i * this.Spacing.y);
      Rays.DrawRay(ray, direction, distance, Color.blue);

      if (Physics2D.RaycastNonAlloc(this.ray, direction, this.hit, distance, this.groundMask) > 0) {
        movement.x = hit[0].point.x - this.ray.x;
        distance = Mathf.Abs(movement.x);
        if (right) {
          movement.x -= this.Inset;
          this.state.Right = true;
        } else {
          movement.x += this.Inset;
          this.state.Left = true;
        }

        if (distance < this.Inset + Rays.Precision) {
          break;
        }
      }
    }
  }

  private void CalculateVertical(ref Vector2 movement) {
    bool up = movement.y > 0;
    float distance = Mathf.Abs(movement.y) + this.Inset;
    Vector2 direction, origin;
    if (up) {
      direction = Vector2.up;
      origin = this.Origins.topLeft;
    } else {
      direction = Vector2.down;
      origin = this.Origins.bottomLeft;
    }

    // raycast from x-pos we will be at (horiz before vert)
    origin.x += movement.x;
    LayerMask mask = groundMask;
    if (!up && !ignorePlatforms) {
      mask |= platformMask;
    }

    for (int i = 0; i < this.VerticalRays; i++) {
      this.ray = new Vector2(origin.x + i * this.Spacing.x, origin.y);
      Rays.DrawRay(this.ray, direction, distance, Color.blue);

      if (Physics2D.RaycastNonAlloc(this.ray, direction, this.hit, distance, mask) > 0) {
        movement.y = hit[0].point.y - this.ray.y;
        distance = Mathf.Abs(movement.y);
        if (up) {
          movement.y -= this.Inset;
          this.state.Above = true;
        } else {
          movement.y += this.Inset;
          this.state.Below = true;
        }

        if (distance < this.Inset + Rays.Precision) {
          break;
        }
      }
    }
  }
}
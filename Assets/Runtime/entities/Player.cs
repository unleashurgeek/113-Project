﻿using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Player : MonoBehaviour {
  private Rigidbody2D rb;

  public LayerMask groundLayer;
  public Transform feetPos;
  [Range(0,20)]
  public float maxSpeed = 5.0f;
  [Range(0,10)]
  public float jumpForce = 10.0f;
  [Range(0,1)]
  public float maxJumpTime = 1.0f;
  public Vector2 feetSize = new Vector2(1f, 0.05f);

  private bool jumping = false;
  private bool jumpHeld = false;
  private float jumpCounter = 0;
  private float xVelocity = 0;

  public void Awake() {
    this.rb = this.GetComponent<Rigidbody2D>();
  }

  private void Update() {
    if (this.xVelocity > 0) {
      this.transform.eulerAngles = Vector3.zero;
    } else if (this.xVelocity < 0) {
      this.transform.eulerAngles = new Vector3(0, 180, 0);
    }
    if (!jumpHeld) {
      this.jumping = false;
    }

    this.xVelocity = 0;
    this.jumpHeld = false;
  }

  private void FixedUpdate() {
    this.rb.velocity = new Vector2(this.xVelocity, this.rb.velocity.y);
  }

  public void Move(float dir) {
    this.xVelocity = dir * this.maxSpeed;
  }

  public void Jump() {
    this.jumpHeld = true;
    if (this.Grounded()) {
      this.rb.velocity = Vector2.up * this.jumpForce;
      this.jumping = true;
      this.jumpCounter = this.maxJumpTime;
    } else if (jumping && this.jumpCounter > 0) {
      this.rb.velocity = Vector2.up * this.jumpForce;
      this.jumpCounter -= Time.deltaTime;
    }
  }

  private bool Grounded() {
    return Physics2D.OverlapBox(this.feetPos.position, this.feetSize, 0, this.groundLayer);
  }
}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyComponent : MonoBehaviour {
    public Animator anim;
    public float deltaX
    {
        get
        {
            return GameManager.Player.transform.position.x - transform.position.x; 
        }
    }
    public float deltaY
    {
        get
        {
            return GameManager.Player.transform.position.y - transform.position.y;
        }
    }

    // flags 
    public bool facingRight;
    public bool canMove;

    void Start () {
        facingRight = true;
        canMove = true; 
	}

    public Vector2 GetDirection()
    {
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        return dir;
    }

    public void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
    }

    public void LookAtTarget()
    {
        if (deltaX < 0 && facingRight || deltaX > 0 && !facingRight) Flip();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Set Movement")]
    public float moveSpeed = 5;
    private float inputX;
    private Rigidbody2D PlayerRB;
    private bool faceRight = true;

    [Header("Set Jump")]
    public float JumpForce = 10;

    [Header("Set GroundCheck")]
    public Transform GroundCheck;
    public float CheckRadius;
    public LayerMask WhatIsGround;
    private bool isGrounded;

    void Start()
    {
        PlayerRB = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        inputX = Input.GetAxis("Horizontal");
        PlayerRB.velocity = new Vector2(inputX * moveSpeed, PlayerRB.velocity.y);

        if (faceRight == false && inputX > 0)
        {
            Flip();
        }
        else if (faceRight == true && inputX < 0)
        {
            Flip();
        }

        isGrounded = Physics2D.OverlapCircle(GroundCheck.position, CheckRadius, WhatIsGround);
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            PlayerRB.velocity = Vector2.up * JumpForce;
        }
    }

    void Flip()
    {
        faceRight = !faceRight;
        Vector3 Scaler = transform.localScale;
        Scaler.x *= -1;
        transform.localScale = Scaler;
    }
}
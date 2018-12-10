﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(MovementController))]
public class Chalice : MonoBehaviour {

  private enum State {Init, Move, FireTell, Fire};

  // TODO: refactor state machine code into a separate StandaloneStateMachine class.
  // Note: should use ReadOnlyDictionary instead, but that is only available in .NET 4 (while we
  // are using .NET 3.5):
  private static readonly Dictionary<State, HashSet<State>> StateTransitions = new Dictionary<State, HashSet<State>> {
    {State.Init, new HashSet<State>{State.Move}},
    {State.Move, new HashSet<State>{State.FireTell}},
    {State.FireTell, new HashSet<State>{State.Fire}},
    {State.Fire, new HashSet<State>{State.Move}}
  };

  [SerializeField]
  private GameObject beamEmitterObject;
  [SerializeField]
  private float firingRange = 4f;
  [SerializeField]
  private float moveDistance = 1f;

  private SpriteRenderer spriteRendererComponent;
  private MovementController movementController;

  private BeamEmitter beamEmitterComponent;
  private State state = State.Init;

  void Start () {
    Assert.IsNotNull(this.beamEmitterObject);

    this.spriteRendererComponent = this.GetComponent<SpriteRenderer>();
    this.movementController = this.GetComponent<MovementController>();

    this.beamEmitterComponent = this.beamEmitterObject.GetComponent<BeamEmitter>();
    Assert.IsNotNull(this.beamEmitterComponent);

    this.ChangeState(State.Move);
  }

  void Update () {
    if(this.state == State.Move) {
      this.spriteRendererComponent.color = Color.white;
      this.AimAtPlayer();
    }
    else if(this.state == State.FireTell) {
      this.spriteRendererComponent.color = Color.yellow;
    }
    else if(this.state == State.Fire) {
      this.spriteRendererComponent.color = Color.blue;
    }
  }

  private void AimAtPlayer() {
    Vector2 direction = GameManager.Player.transform.position - this.transform.position;
    direction.Normalize();
    Vector3 directionUpwards = Vector3.Cross(new Vector3(direction.x, direction.y, 0), -Vector3.forward);
    this.transform.rotation = Quaternion.LookRotation(Vector3.forward, directionUpwards);
  }

  private void ChangeState(State newState) {
    if(!Chalice.StateTransitions[this.state].Contains(newState)) {
      throw new ArgumentException(String.Format("Cannot transition from State {0} to State {1}",
                                                this.state, newState));
    }

    // State exit actions:
    // None currently.

    // State entry actions:
    if(newState == State.Move) {
      StartCoroutine(MoveCoroutine());
    }
    else if(newState == State.FireTell) {
      StartCoroutine(PauseThenFireCoroutine());
    }
    else if(newState == State.Fire) {
      StartCoroutine(FireThenMoveStateCoroutine());
    }

    this.state = newState;
  }

  private IEnumerator PauseThenFireCoroutine() {
    yield return new WaitForSeconds(1);
    this.ChangeState(State.Fire);
  }

  private IEnumerator FireThenMoveStateCoroutine() {
    this.beamEmitterComponent.Fire();
    yield return new WaitUntil(() => !this.beamEmitterComponent.IsFiring);
    this.ChangeState(State.Move);
  }

  private IEnumerator MoveCoroutine() {
    bool done = false;
    while(!done) {
      this.movementController.Move((GameManager.Player.transform.position - this.transform.position).normalized * this.moveDistance);
      yield return new WaitForSeconds(1);
      if(this.InFiringRange()) {
        this.ChangeState(State.FireTell);
        done = true;
      }
    }
  }

  private bool InFiringRange() {
    return (GameManager.Player.transform.position - this.transform.position).magnitude <= this.firingRange;
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public struct Position {
    public double time;
    public Vector2 position;
}

public class PlayerMovement : NetworkBehaviour {
    [SerializeField] private float speed = 0.1f;
    private Rigidbody2D rb;
    private List<Position> previousPositions = new List<Position>();
    private Transform t;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    [Command]
    public void Cmd_ServerMovePlayer(float _x, float _y) {
        MovePlayer(_x, _y);
        Rpc_ClientsMovePlayer(rb.position);
    }

    [ClientRpc]
    private void Rpc_ClientsMovePlayer(Vector2 _pos) {
        if (!isLocalPlayer)
            MovePlayer(_pos);
        else if (isLocalPlayer)
            ReconcileToServer(_pos);
    }

    private void MovePlayer(Vector2 _position) {
        rb.MovePosition(_position);
    }

    private void MovePlayer(float _x, float _y) {
        rb.MovePosition(new Vector2(rb.position.x + ( _x * speed ), rb.position.y + ( _y *
        speed )));
    }

    public void MovementPrediction(float _x, float _y) {
        Position pos = new Position();
        pos.time = Time.time;
        rb.MovePosition(new Vector2(rb.position.x + ( _x * speed ), rb.position.y + ( _y * speed )));
        pos.position = t.position;
        previousPositions.Add(pos);
    }

    private void ReconcileToServer(Vector2 _position) {
        for (int i = 0; i < previousPositions.Count; i++) {
            if (_position == previousPositions[i].position) {
                previousPositions.RemoveRange(0, i);
                return;
            }
        }

        previousPositions.Clear();
        MovePlayer(_position);
    }
}

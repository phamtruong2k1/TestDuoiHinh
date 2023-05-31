using UnityEngine;

public class Aim : MonoBehaviour //Component class to set the trajectory of the aim
{

    private float speed;  //Aim speed
    private Vector2 leftBounds; //Left and bottom sides of the image square
    private Vector2 rightBounds; //Right and upper sides of the image square
    private Vector2 moveToPoint; //Point on the side of the image square which the aim should move to
    private int currentSide; //There are 4 sides of the square. 1=bottom, 2=right, 3=upper, 4=left. This variable refers to the current side that aim is moving to
    private float sideHalfSizeX; //Side is devided on two halfs. The next move point is calculated depending on which half is aim moving from
    private float sideHalfSizeY;

    public void OnStart(Vector2 leftBounds, Vector2 rightBounds) //Initializing method called from the PlanksManager
    {
        this.leftBounds = leftBounds;
        this.rightBounds = rightBounds;
        sideHalfSizeX = Mathf.Abs(rightBounds.x - leftBounds.x) / 1.5f; //1.5(not 2)to prevent aim moving only to specific halfs of the sides.
        sideHalfSizeY = Mathf.Abs(rightBounds.y - leftBounds.y) / 1.5f;
        speed = GameController.Instance.AimSpeed;
        currentSide = 2; //What side is aim going at start
        SetMovePoint(currentSide); //Set the next point on the side to move

    }
    private void Update()
    {
        //Move the aim while it is not in the destination
        if ((Vector2)gameObject.transform.position != moveToPoint)
        {
            gameObject.transform.position = Vector2.MoveTowards(gameObject.transform.position, moveToPoint, speed * Time.deltaTime);
        }
        else
        {
            //When the aim hits the side, calculate next movepoint
            if (currentSide != 4)
            {
                SetMovePoint(++currentSide);
            }
            else
            {
                currentSide = 1;
                SetMovePoint(currentSide);
            }
        }
    }

    private void SetMovePoint(int side) //Pseudorandomly calculate next movepoint depending on what side and its half of the square is reached
    {
        switch (side)
        {
            case 1: //Current side = bottom

                if (Mathf.Abs(rightBounds.y - gameObject.transform.position.y) < Mathf.Abs(leftBounds.y - gameObject.transform.position.y)) //Check which half of the side is reached
                {
                    moveToPoint = new Vector2(Random.Range(rightBounds.x, rightBounds.x - sideHalfSizeX), leftBounds.y);
                }
                else
                {
                    moveToPoint = new Vector2(Random.Range(leftBounds.x, leftBounds.x + sideHalfSizeX), leftBounds.y);
                }
                break;
                
            case 2: // Current side = right

                if (Mathf.Abs(rightBounds.x - gameObject.transform.position.x) < Mathf.Abs(leftBounds.x - gameObject.transform.position.x))//Check which half of the side is reached
                {
                    moveToPoint = new Vector2(rightBounds.x, Random.Range(leftBounds.y, leftBounds.y + sideHalfSizeY));
                }
                else
                {
                    moveToPoint = new Vector2(rightBounds.x, Random.Range(rightBounds.y, rightBounds.y - sideHalfSizeY));
                }
                break;

            case 3: // Current side = upper

                if (Mathf.Abs(rightBounds.y - gameObject.transform.position.y) < Mathf.Abs(leftBounds.y - gameObject.transform.position.y))//Check which half of the side is reached
                {
                    moveToPoint = new Vector2(Random.Range(rightBounds.x, rightBounds.x - sideHalfSizeX), rightBounds.y);
                }
                else
                {
                    moveToPoint = new Vector2(Random.Range(leftBounds.x, leftBounds.x + sideHalfSizeX), rightBounds.y);
                }
                break;

            case 4:  // Current side = left

                if (Mathf.Abs(rightBounds.x - gameObject.transform.position.x) < Mathf.Abs(leftBounds.x - gameObject.transform.position.x))//Check which half of the side is reached
                {
                    moveToPoint = new Vector2(leftBounds.x, Random.Range(leftBounds.y, leftBounds.y + sideHalfSizeY));
                }
                else
                {
                    moveToPoint = new Vector2(leftBounds.x, Random.Range(rightBounds.y, rightBounds.y - sideHalfSizeX));
                }
                break;

            default:
                break;
        }
    }
}

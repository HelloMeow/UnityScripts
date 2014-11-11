using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class XYUIScrollGrid : MonoBehaviour {

    public UIGrid mBaseGrid;
    public UIScrollView mScrollView;
    public delegate int DelegateGetCellCount();
    public delegate void DelegateUpdateCellAtIndex(GameObject go, int index);
    public GameObject mCellPrefab;
    public float cellWidth, cellHeight;
    public int maxPerLine = 1;
    public bool disableDragIfFits = true;
    public bool keepWithinThePanel = true;
    public bool useCellBoxColliderSize = true;
    public enum Arrangement {
        Horizontal,
        Vertical,
    }

    public Arrangement mArrangement;

    public enum Movement {
        Horizontal,
        Vertical,
    }

    public Movement mMovement;

    public DelegateGetCellCount GetCellCount;
    public DelegateUpdateCellAtIndex UpdateCellAtIndex;
    public enum Alignment {
        Left,
        Top,
        Centered,
        Right,
        Bottom,
    }

    public Alignment alignment = Alignment.Centered;

    private bool mInitDone = false;
    private List<GameObject> mCells;
    private bool mNeedReposition = true;

    private void init() {

        // movement
        if (mMovement == Movement.Horizontal)
            mScrollView.movement = UIScrollView.Movement.Horizontal;
        else if (mMovement == Movement.Vertical)
            mScrollView.movement = UIScrollView.Movement.Vertical;

        // restrict with panel
        mScrollView.restrictWithinPanel = keepWithinThePanel;
        mBaseGrid.keepWithinPanel = keepWithinThePanel;

        // disable drag if fits
        mScrollView.disableDragIfFits = disableDragIfFits;

        // Arrangement
        if(mArrangement == Arrangement.Horizontal) {
            mBaseGrid.arrangement = UIGrid.Arrangement.Horizontal;
        } else {
            mBaseGrid.arrangement = UIGrid.Arrangement.Vertical;
        }

        // Max items per-line
        mBaseGrid.maxPerLine = maxPerLine;

        // cell width & height
        mBaseGrid.cellHeight = cellHeight;
        mBaseGrid.cellWidth = cellWidth;
        // use cell's boxCollider size
        if(useCellBoxColliderSize) {
            BoxCollider bc = mCellPrefab.GetComponent<BoxCollider>() ??
                mCellPrefab.GetComponentInChildren<BoxCollider>();
            if(bc) {
                mBaseGrid.cellWidth = cellWidth = bc.size.x;
                mBaseGrid.cellHeight = cellHeight = bc.size.y;
            }
        }

        // cell prefab
        if(mCellPrefab.GetComponent<UIDragScrollView>() != null) {
            mCellPrefab.GetComponent<UIDragScrollView>()
                       .scrollView = mScrollView;
        } else {
            mCellPrefab.GetComponentInChildren<UIDragScrollView>()
                       .scrollView = mScrollView;
        }

        mCells = new List<GameObject>();

        mBaseGrid.onReposition = delegate {

            if (!mNeedReposition) {
                return;
            }
            mNeedReposition = false;

            // Align grid's position by Alignment settings.
            UIPanel panel = mScrollView.GetComponent<UIPanel>();

            Bounds cellBounds = NGUIMath.CalculateRelativeWidgetBounds(mCellPrefab.transform, true);

            bool isHorizontal = mBaseGrid.arrangement == UIGrid.Arrangement.Horizontal;
            float RW = isHorizontal ? panel.finalClipRegion.z : panel.finalClipRegion.w; // visible region width
            float RH = isHorizontal ? panel.finalClipRegion.w : panel.finalClipRegion.z;
            float CW = isHorizontal ? cellWidth : cellHeight; // cell width
            float CH = isHorizontal ? cellHeight : cellWidth;
            float SX = isHorizontal ? panel.clipSoftness.x : panel.clipSoftness.y; // clip softness x
            float SY = isHorizontal ? panel.clipSoftness.y : panel.clipSoftness.x;
            float CX = isHorizontal ? panel.finalClipRegion.x : panel.finalClipRegion.y; // center x
            float CY = isHorizontal ? panel.finalClipRegion.y : panel.finalClipRegion.x;
            float DX = isHorizontal ? 0 : (cellHeight - cellBounds.size.y) / 2.0f;
            float DY = isHorizontal ? (cellHeight - cellBounds.size.y) / 2.0f : 0;
            int sign = isHorizontal ? 1 : -1;

            Debug.Log("RW["+RW+"]"+
                "RH["+RH+"]"+
                "CW["+CW+"]"+
                "CH["+CH+"]"+
                "SX["+SX+"]"+
                "SY["+SY+"]"+
                "CX["+CX+"]"+
                "SX["+DX+"]"+
                "DY["+DY+"]"+
                "BW["+cellBounds.size.x+"]"+
                "BH[" + cellBounds.size.y+ "]");


            float x, y;
            switch(alignment) {
                case Alignment.Left:
                case Alignment.Top: {
                    x = sign * (CW / 2.0f - RW / 2.0f + SX - DX) + CX;
                    break;
                }
                default:
                case Alignment.Centered: {
                    x = sign * (CW / 2.0f - mBaseGrid.maxPerLine * CW / 2.0f - DX) + CX;
                    break;
                }
                case Alignment.Bottom:
                case Alignment.Right: {
                    x = sign * (RW / 2.0f - mBaseGrid.maxPerLine * CW + CW / 2.0f - SX - DX) + CX;
                    break;
                }

            }
            y = sign * (RH / 2.0f - CH / 2.0f - SY + DY) + CY;
            
            mBaseGrid.transform.localPosition =
                new Vector3(isHorizontal ? x : y, isHorizontal ? y : x, mBaseGrid.transform.localPosition.z);

            mScrollView.Press(true);
            mScrollView.Press(false);
            mScrollView.RestrictWithinBounds(true, true, true);

        };

        mInitDone = true;
    }

    public void ReloadData() {
        if(!mInitDone)
            init();

        mCells.Clear();
        while(mBaseGrid.transform.childCount > 0)
            NGUITools.Destroy(mBaseGrid.transform.GetChild(0).gameObject);

        int cellCount = GetCellCount();
        for(int i = 0; i < cellCount; i++) {
            GameObject cellObj = NGUITools.AddChild(mBaseGrid.gameObject, mCellPrefab);
            cellObj.SetActive(true);
            UpdateCellAtIndex(cellObj, i);
            mCells.Add(cellObj);
        }

        mBaseGrid.repositionNow = true;
    }
}

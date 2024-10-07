using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DanceGraph.SignalBehaviours;

using System.Linq;

namespace DanceGraph {
    public class ClientEntry {
        public ClientEntry() {
        }
        
        public ClientEntry(int c, int ci) {
            this.Class = c;
            this.ClassIdx = ci;
        }
        
        public int Class { get; set; }
        public int ClassIdx { get; set; }
    };



    public class DanceSlots {

        //private Dictionary<int, int> dancerList = new Dictionary<int, int>();

        // A list of slots with the dancer clientIdx in the
        private List<int> dancerList = new List<int>();

        public int GetDancer(int userIdx) {
            // Returns the slot the dancer is using, or -1 if it's not there
            int iidx = dancerList.IndexOf(userIdx);
            return iidx;
        }
		
        public int AddDancer(int userIdx) {
            int dslot = dancerList.IndexOf(userIdx);

            // Dancer's already here, just pass on the slot index and do nothing
            if (dslot >= 0)
                return dslot;
			
            // Dancer isn't here, find the smallest empty slot and put them in

            for (int i = 0; i < dancerList.Count; i++) {
                if (dancerList[i] < 0) {
                    dancerList[i] = userIdx;
                    return i;
                }
            }

            // All the slots up to the end of the list are taken. Add the user to the end of the list
            dancerList.Add(userIdx);
            return (dancerList.Count - 1);
        }

        public bool RemoveDancer(int userIdx) {
            int dslot = dancerList.IndexOf(userIdx);

            // We remove by setting the slot value to -1, rather than resizing the list which will be confusing
            if (dslot > 0) {
                dancerList[dslot] = -1;
                return true;
            }

            // No dancer to remove. Return false to signify that fact
            return false;
        }
		
    };
    
    abstract public class PlacementProvider {
        // Updates the client type of the given user
        // Returns a boolean set to true if the user has changed type (and needs to be moved)
        public abstract bool SetClass(int userIdx, int clientType);        

        // Returns -1 if there's no existing class
        public abstract int GetClass(int userIdx);
		
        public abstract Adjustment DancePlacement(int idx,
                                                  Transform tForm,
                                                  Transform avForm,
                                                  Vector3 zPos,
                                                  Quaternion zRot);
        
        
    };


    public class PairlineProvider : PlacementProvider {

        protected int default_slot = 1;


        // Default, Everyone
        DanceSlots [] slotList  = { new DanceSlots(), new DanceSlots() };

        protected Dictionary <int, ClientEntry> clientDict = new Dictionary<int, ClientEntry>();
        
        public override bool SetClass(int userIdx, int clientType) {
            //int pType = (clientType > 1) ? (int)ClientType.HUMAN_USER : clientType;
            int pType = (int)ClientType.HUMAN_USER;
            
            try {
                int oldType = clientDict[userIdx].Class;
                if (oldType == pType) {
                    Debug.Log($"Client {userIdx} type unchanged from {oldType}, disregarding");
                    return false;
                }
                else {
                    slotList[oldType].RemoveDancer(userIdx);
                    int newslot = slotList[clientType].AddDancer(userIdx);
                    Debug.Log($"Client class for {userIdx} updated to {pType}, slot {newslot}");
                    clientDict[userIdx] = new ClientEntry(pType, newslot);
                    //clientDict.Add(userIdx, new ClientEntry(clientType, newslot));
                    return false;
                }
                
            }
            catch(KeyNotFoundException) {
                int newslot = slotList[pType].AddDancer(userIdx);
                clientDict.Add(userIdx, new ClientEntry(pType, newslot));
                Debug.Log($"Client class for {userIdx} initialized to {clientType}, slot {newslot}");
                return true;
            }
        }

		
        public override int GetClass(int userIdx) {
            try {
                ClientEntry ce = clientDict[userIdx];
                return ce.Class;
            }
            catch(KeyNotFoundException) {
                return -1;
            }
        }


		
        protected Adjustment GetPosition(int cIdx, int userType) {

            const float kIntraPairDistance = 0.75f; // distance between a pair of dancers
            const float kInterPairDistance = 2.0f; // distance between pairs of dancers
            bool rotate180 = (cIdx & 1) == 1;
            
            // we want to achieve indexing: 6 4 2 0 1 3 5
            int pairIndex = cIdx >> 1;
            bool pairIsOdd = (pairIndex & 1) == 1;
            int absSlotIndex = (pairIndex + 1) >> 1;
            float x = kInterPairDistance * absSlotIndex * (pairIsOdd ? 1.0f : -1.0f);
            float y = World.instance.zedAvatarRootHeight; // move everybody up 1 unit
            float z = 0.5f * kIntraPairDistance * (rotate180 ? -1.0f : 1.0f);

            Adjustment pos = new Adjustment();


            pos.rotate180 = rotate180;
            pos.rotAngle = rotate180 ? 180 : 0;
            pos.startPosition = new Vector3(x, World.instance.zedAvatarRootHeight, z);
            pos.delta = new Vector3(0.0f, 0.0f, 0.0f);
            
			return pos;
        }

        private float rotAngle(int t, int tType, int f, int fromType) {
            Vector3 fromPos = GetPosition(f, fromType).startPosition;
            Vector3 toPos = GetPosition(t, tType).startPosition;
            Vector3 pdiff = toPos - fromPos;

            // Swap x and z coords because we're getting the clockwise rotation from bearing 0
            float theta = (180.0f * (float) Math.Atan2(pdiff[0], pdiff[2])) / (float) Math.PI;
            //return 180.0f * theta / (float) Math.PI;
            Debug.Log($"User in slot {f} at {fromPos} Looking at {t} at {toPos} rot {theta}");
            return theta;
        }
		
        public override Adjustment DancePlacement(int idx,
                                                  Transform tForm,
                                                  Transform avForm,
                                                  Vector3 zPos,
                                                  Quaternion zRot) {


            ClientEntry ce = clientDict[idx];
            int clientType = ce.Class;
            int danceSlot = ce.ClassIdx;

			
            Adjustment adj = GetPosition(danceSlot, clientType);

            Transform cForm = avForm.FindDeepChild("mixamorig:Hips");
            adj.delta  = adj.startPosition - cForm.position;
            adj.delta[1] = 0.0f;

            tForm.position = adj.startPosition;

            float rotationAngle = rotAngle(danceSlot, clientType, danceSlot ^ 1, clientType);
            adj.rotAngle = rotationAngle;

            //tForm.RotateAround(tForm.position, Vector3.up, adj.rotAngle);

            // targetPosition[1] = World.instance.zedAvatarRootHeight;
            // adj.startPosition = targetPosition;
				
            // Vector3 delta = adj.startPosition - cForm.position;
            // delta[1] = 0.0f;

            // tForm.position = adj.startPosition;

            tForm.RotateAround(cForm.position, Vector3.up, adj.rotAngle);
            
            Debug.Log($"Linear: Adjusting avatar {idx} ({clientType}/{danceSlot}) to Position {adj.startPosition}, rotation {adj.rotAngle}, delta {adj.delta}");		
            return adj;
        }

        
    };

    public class GamesComProvider : PlacementProvider {
        // We use 2 classes, humans and bots. 

        // Default, Human, Bot
        DanceSlots [] slotList  = { new DanceSlots(), new DanceSlots(), new DanceSlots()};
        protected Dictionary <int, ClientEntry> clientDict = new Dictionary<int, ClientEntry>();

        public override bool SetClass (int userIdx, int clientType) {

            int pType = (clientType > 2) ? (int)ClientType.DEFAULT_CLIENT : clientType;
			
            try {
                int oldType = clientDict[userIdx].Class;
                if (oldType == pType) {
                    Debug.Log($"Client {userIdx} type unchanged from {oldType}, disregarding");
                    return false;
                }
                else {
                    slotList[oldType].RemoveDancer(userIdx);
                    int newslot = slotList[clientType].AddDancer(userIdx);
                    //clientDict.Add(userIdx, new ClientEntry(clientType, newslot));
                    clientDict[userIdx] = new ClientEntry(pType, newslot);
                    Debug.Log($"Client class for {userIdx} updated to {pType}, slot {newslot}");					
                    return true;
                }
                
            }
            catch(KeyNotFoundException ex) {
                int newslot = slotList[pType].AddDancer(userIdx);
                clientDict.Add(userIdx, new ClientEntry(pType, newslot));
                Debug.Log($"Client class for {userIdx} initialized to {pType}, slot {newslot}");									
                return true;
            }
        }

        public override int GetClass(int userIdx) {
            try {
                ClientEntry ce = clientDict[userIdx];
                return ce.Class;
            }
            catch(KeyNotFoundException) {
                return -1;
            }
        }
		

        private static Vector3 [] botPositions = new Vector3[] {

            new Vector3 (-5.5f, 0.0f, -2.1f),        
            new Vector3 (-5.0f, 0.0f, -1.9f),

            new Vector3 (5.0f, 0.0f, -1.97f),        
            new Vector3 (4.5f, 0.0f, -2.17f),

            new Vector3 (4.0f, 0.0f, 5.1f),				
            new Vector3 (3.5f, 0.0f, 4.9f),

            // new Vector3 (-5.5f, 0.0f, -1.5f),        
            // new Vector3 (-5.0f, 0.0f, -2.5f),

            // new Vector3 (5.0f, 0.0f, -1.57f),        
            // new Vector3 (4.5f, 0.0f, -2.57f),

            // new Vector3 (4.0f, 0.0f, 5.5f),				
            // new Vector3 (3.5f, 0.0f, 4.5f),
			

			
        };

        private static Vector3 [] humanPositions = new Vector3 [] {
            new Vector3 (0f, 0.0f, 0f),
            new Vector3 (0.0f, 0.0f, 1.0f)
        };

        private static Vector3 [] defaultPositions = new Vector3 [] {
            new Vector3 (0.0f, 0.0f, 50.0f),
            new Vector3 (0.0f, 0.0f, 51.0f)		   
        };
		
        private List<Vector3 []> posLists = new List<Vector3 []> { defaultPositions,
                                                                   humanPositions,
                                                                   botPositions};
		

        private Vector3 getPosition(int clientIdx, int clientClass) {
			
            Vector3 [] posList = posLists[clientClass];

            int side = clientIdx % posList.Length;
            int div = clientIdx % posList.Length;			
			
            Vector3 basePos = posList[side];
			
            if (clientIdx > posList.Length) {
                basePos.x += 50 + 5 * div;
            }
			
            return basePos;
        }

		

        private Vector3 lookDir(int t, int tType, int f, int fromType) {
            Vector3 fromPos = getPosition(f, fromType);
            Vector3 toPos = getPosition(t, tType);
            Vector3 pdiff = toPos - fromPos;
            return pdiff.normalized;
        }

		
        private float rotAngle(int t, int tType, int f, int fromType) {
            Vector3 fromPos = getPosition(f, fromType);
            Vector3 toPos = getPosition(t, tType);
            Vector3 pdiff = toPos - fromPos;

            // Swap x and z coords because we're getting the clockwise rotation from bearing 0
            float theta = (180.0f * (float) Math.Atan2(pdiff[0], pdiff[2])) / (float) Math.PI;
            //return 180.0f * theta / (float) Math.PI;
            Debug.Log($"User in slot {f} at {fromPos} Looking at {t} at {toPos} rot {theta}");
            return theta;
        }
		
        public override Adjustment DancePlacement(int index,
                                                  Transform tForm,
                                                  Transform avForm,
                                                  Vector3 zPos,
                                                  Quaternion zRot) {

            Adjustment adj = new Adjustment();
            adj.rotate180 = false;
        
            adj.delta = new Vector3(0f, 0f, 0f);

            ClientEntry ce;
            try {
                ce = clientDict[index];
            }
            catch(KeyNotFoundException) {
                ce = new ClientEntry(0,0);
            }
            int clientType = ce.Class;
            int clientIdx = ce.ClassIdx;			
			


            Vector3 targetPosition = getPosition(clientIdx, clientType);
            Debug.Log($"Demo: Placing client {index} as class {clientType}, slot {clientIdx}, pos {adj.startPosition}");			
#if false

            // Everyone is paired off
            int partner = clientIdx ^ 1;
            float rotationAngle = rotAngle(clientIdx, clientType, partner, clientType);
            //Vector3 lDir = lookDir(clientIdx, clientType, partner, clientType);
#else
            float rotationAngle = rotAngle(clientIdx, clientType, 0, (int) ClientType.HUMAN_USER);
            //Vector3 lDir = lookDir(clientIdx, clientType, 0, (int) ClientType.HUMAN_USER);
#endif
            adj.rotAngle = rotationAngle;

            Transform cForm = avForm.FindDeepChild("mixamorig:Hips");

            targetPosition[1] = World.instance.zedAvatarRootHeight;
            adj.startPosition = targetPosition;
				
            Vector3 delta = adj.startPosition - cForm.position;
            delta[1] = 0.0f;

            tForm.position = adj.startPosition;

            tForm.RotateAround(cForm.position, Vector3.up, adj.rotAngle);
            /*
              Transform cForm = avForm.FindDeepChild("mixamorig:Hips");
              adj.delta = adj.startPosition - cForm.position;
              adj.delta[1] = 0.0f;
              //tForm.position = adj.startPosition + adj.delta;
              tForm.Translate(adj.delta);
            */


            Debug.Log($"DPGC: Adjusting avatar {index} ({clientType}/{clientIdx}) to Position {adj.startPosition}, rotation {rotationAngle}, delta {adj.delta}");
            return adj;
        }
    };



    public class SpiralProvider: PlacementProvider {

		private Vector3 fudgeFactor = new Vector3(0.0f, 0.0f, -1.5f);
		
        // Angle difference between users in radians
        public float thetaDifference = 1.0f;

        // Radius difference between users        
        public float radiusDifference = 0.25f;

        // Initial distance of the nearest bot to the centre
        public float initialRadius = 1.5f;
		
        DanceSlots [] slotList  = { new DanceSlots(), new DanceSlots(), new DanceSlots()};
        protected Dictionary <int, ClientEntry> clientDict = new Dictionary<int, ClientEntry>();

        private static Vector3 [] humanPositions = new Vector3 [] {
            // new Vector3 (0.0f, 0.0f, -0.25f),
            // new Vector3 (0.0f, 0.0f, 0.25f)
            new Vector3 (0.0f, 0.0f, -0.05f),
            new Vector3 (0.0f, 0.0f, 0.05f),			
        };

        private Vector3 getPosition(int clientIdx, int clientClass, bool fudged = false) {
            if (clientClass == (int)ClientType.HUMAN_USER) {
				Vector3 retval = humanPositions[clientIdx % 2] +  new Vector3(2.0f * Mathf.Floor(clientIdx / 2.0f), 0.0f, 0.0f);
				if (fudged) {
					Debug.Log($"Returning {clientIdx}/{clientClass} fudged from {retval} to {retval + fudgeFactor}");					
					return retval + fudgeFactor;
				}
				else {
					Debug.Log($"Returns {clientIdx}/{clientClass} unfudged {retval}");
					return retval;
				}
            }
            else if (clientClass == (int) ClientType.DEMO_BOT) {
                //Spiral outwards
                float theta = thetaDifference * clientIdx;
                float radius = radiusDifference * clientIdx + initialRadius;

                return new Vector3(radius * Mathf.Cos(theta), 0.0f, radius * Mathf.Sin(theta));
            }
            else
				if (fudged) {
					
					Vector3 retval = humanPositions[clientIdx % 2] +  new Vector3(2.0f * ((float)clientIdx / 2.0f), 0.0f, 40.0f) + fudgeFactor;

					return retval;
				}
				else {
					Vector3 retval = humanPositions[clientIdx % 2] +  new Vector3(2.0f * ((float)clientIdx / 2.0f), 0.0f, 40.0f);
					return retval;
				}
		}

        // private Vector3 getPosition(int clientIdx, int clientClass, bool fudged = false) {
        //     if (clientClass == (int)ClientType.HUMAN_USER) {
        //         return humanPositions[clientIdx % 2] +  new Vector3(2.0f * Mathf.Floor(clientIdx / 2.0f), 0.0f, 0.0f);
        //     }
        //     else if (clientClass == (int) ClientType.DEMO_BOT) {
        //         //Spiral outwards
        //         float theta = thetaDifference * clientIdx;
        //         float radius = radiusDifference * clientIdx + initialRadius;

        //         return new Vector3(radius * Mathf.Cos(theta), 0.0f, radius * Mathf.Sin(theta));
        //     }
        //     else
		// 		return humanPositions[clientIdx % 2] +  new Vector3(2.0f * ((float)clientIdx / 2.0f), 0.0f, 40.0f);					
		// }
		
        public override bool SetClass(int userIdx, int clientType) {
            int pType = (clientType > 2) ? (int)ClientType.DEFAULT_CLIENT : clientType;
			
            try {
                int oldType = clientDict[userIdx].Class;
                if (oldType == pType) {
                    Debug.Log($"Placement: Client {userIdx} type unchanged from {oldType}, disregarding");
                    return false;
                }
                else {
                    slotList[oldType].RemoveDancer(userIdx);
                    int newslot = slotList[clientType].AddDancer(userIdx);
                    //clientDict.Add(userIdx, new ClientEntry(clientType, newslot));
                    clientDict[userIdx] = new ClientEntry(pType, newslot);
                    Debug.Log($"Placement: Client class for {userIdx} updated to {pType}, slot {newslot}");					
                    return true;
                }
                
            }
            catch(KeyNotFoundException ex) {
                int newslot = slotList[pType].AddDancer(userIdx);
                clientDict.Add(userIdx, new ClientEntry(pType, newslot));
                Debug.Log($"Placement: Client class for {userIdx} initialized to {pType}, slot {newslot}");									
                return true;
            }
        }

        // Ugh. yBotAvatar scale is 99
        private Vector3 fudgefactor(Transform tForm) {
            Vector3 fudge = -99.0f * new Vector3(tForm.localPosition.x, 0.0f, tForm.localPosition.y);
            return fudge;
        }

        
        private float rotAngle(int t, int tType, int f, int fromType) {
            Vector3 fromPos = getPosition(f, fromType, World.instance.placementFudge);
            Vector3 toPos = getPosition(t, tType, World.instance.placementFudge);
            Vector3 pdiff = toPos - fromPos;

            // Swap x and z coords because we're getting the clockwise rotation from bearing 0
            float theta = (180.0f * (float) Math.Atan2(pdiff[0], pdiff[2])) / (float) Math.PI;
            //return 180.0f * theta / (float) Math.PI;
            Debug.Log($"Placement (Spiral): User in slot {f} at {fromPos} Looking at {t} at {toPos} rot {theta}");
            return theta;
        }

		private float rotAngle(Vector3 toPos, Vector3 fromPos) {
            Vector3 pdiff = toPos - fromPos;
            float theta = (180.0f * (float) Math.Atan2(pdiff[0], pdiff[2])) / (float) Math.PI;
            Debug.Log($"Placement (Spiral): Raw rotation change");			
			return theta;
		}

		
        public override int GetClass(int userIdx) {
            try {
                ClientEntry ce = clientDict[userIdx];
                return ce.Class;
            }
            catch(KeyNotFoundException) {
                return -1;
            }
        }


		
        public override Adjustment DancePlacement(int index,
                                                  Transform tForm,
                                                  Transform avForm,
                                                  Vector3 zPos,
                                                  Quaternion zRot) {

            Adjustment adj = new Adjustment();
            adj.rotate180 = false;
        
            adj.delta = new Vector3(0f, 0f, 0f);

            ClientEntry ce;
            try {
                ce = clientDict[index];
            }
            catch(KeyNotFoundException) {
                ce = new ClientEntry(0,0);
            }
            int clientType = ce.Class;
            int clientIdx = ce.ClassIdx;			
			

            Vector3 targetPosition = getPosition(clientIdx, clientType, World.instance.placementFudge);
			targetPosition[1] = World.instance.zedAvatarRootHeight;
            adj.startPosition = targetPosition;

			tForm.position = adj.startPosition;
			
            Debug.Log($"Placement: Spiral: Placing client {index} as class {clientType}, slot {clientIdx}, pos {targetPosition}");			
#if true
			float rotationAngle;
			if (clientType == 1) {
            // Everyone is paired off
				int partner = clientIdx ^ 1;
				rotationAngle = rotAngle(clientIdx, clientType, partner, clientType);
			}
			else if (clientIdx > 0) {
				rotationAngle = rotAngle(clientIdx, clientType, 0, (int) ClientType.HUMAN_USER);
			}
			else {
				rotationAngle = rotAngle(clientIdx, clientType, 1, (int) ClientType.HUMAN_USER);				
			}
#else
			// Intentionally left blank
#endif
            adj.rotAngle = rotationAngle;


			
            Transform cForm = avForm.FindDeepChild("mixamorig:Hips");
			Debug.Log($"GamePositions #1: tForm: {tForm.position}, avForm: {avForm.position}, Hips: {cForm.position}");
            Vector3 delta = adj.startPosition - cForm.position;
            delta[1] = 0.0f;
            
            
            Debug.Log($"Hip position is {cForm.position}, want to hit {adj.startPosition}");
            
            tForm.position = adj.startPosition;
            //Debug.Log($"Spiral: {index} Rotating tForm {tForm.position} around {avForm.position}");			

            tForm.RotateAround(tForm.position, Vector3.up, adj.rotAngle);

			Vector3 finalDisplacement = (adj.startPosition - cForm.position);
			tForm.position += new Vector3(finalDisplacement.x, World.instance.zedAvatarRootHeight - tForm.position.y, finalDisplacement.z);

			
			
            /*
              Transform cForm = avForm.FindDeepChild("mixamorig:Hips");
              adj.delta = adj.startPosition - cForm.position;
              adj.delta[1] = 0.0f;

              tForm.Translate(adj.delta);
            */

			Transform yB = avForm.FindDeepChild("ybotAvatar");


			
			Debug.Log($"GamePositions #2: tForm: {tForm.position}, avForm: {avForm.position}, yBot: {yB.position}, Hips: {cForm.position}");
            Debug.Log($"Placement: Adjusting avatar {index} ({clientType}/{clientIdx}) to Position {adj.startPosition}, rotation {rotationAngle}, delta {adj.delta}, zPos {zPos}");

            return adj;
        }
    };
};

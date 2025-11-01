# Unity Setup Instructions (Beginner-Friendly)

## ✅ Your Current Situation
You opened the project in Unity and see nothing in the Game tab. **This is normal!** The scripts exist but the scene needs to be set up.

## 🎯 What You Need to Do (5 Simple Steps)

### Step 1: Open the Correct Scene
1. In Unity, look at the top menu
2. Click: **File → Open Scene**
3. Navigate to: `Assets/Scenes/SampleScene.unity`
4. Click **Open**

You should now see an empty scene with a camera icon.

### Step 2: Create the Setup GameObject
1. In the **Hierarchy** window (left side of Unity):
   - Right-click in empty space
   - Select: **Create Empty**
   - A new GameObject appears called "GameObject"
   - Click on it to select it
   - Press **F2** to rename it
   - Type: `SceneSetup`
   - Press **Enter**

### Step 3: Add the SceneSetup Component
1. With "SceneSetup" still selected in Hierarchy
2. Look at the **Inspector** window (right side of Unity)
3. At the bottom of Inspector, click the button: **Add Component**
4. In the search box that appears, type: `Scene Setup`
5. Click on **Scene Setup** when it appears in the list

You should now see the SceneSetup component in the Inspector!

### Step 4: Click the Magic Button!
1. In the Inspector, you'll see the SceneSetup component
2. You'll see text that says: "Click the button below to auto-generate the game scene"
3. Below that, there's a big button: **Setup Game Scene**
4. **Click that button!**

✨ Magic happens! Unity will create all the game objects automatically.

### Step 5: Press Play
1. At the top-center of Unity, there's a **Play** button (▶)
2. Click it!

## 🎮 What You Should See

After pressing Play, you should see:
- ✅ A hexagonal grid with different colored cells
  - Green = Plains
  - Dark Green = Forest
  - Gray = Mountains
  - Blue = Water
- ✅ **2 Blue cylinders** (your units) in the bottom-left corner
- ✅ **2 Red cylinders** (enemy AI units) in the top-right corner
- ✅ In the **Console** window (bottom of Unity), messages like:
  - "Game initialized! Player 0's turn."
  - "Spawned Infantry for Player 0 at (0,1)"

## 🎮 How to Play

### Controls:
- **Left Click** on a blue unit → Select it (you'll see debug message)
- **Left Click** on an empty cell → Move the selected unit there
- **Left Click** on a red unit → Attack it (if in range)
- **SPACE** → End your turn (AI will play automatically)
- **Mouse Wheel** → Zoom in/out
- **Middle Mouse + Drag** → Pan camera
- **W/A/S/D** → Move camera
- **Q/E** → Rotate camera

### Game Info:
All game information appears in the **Console** window:
- Turn numbers
- Who's turn it is
- Resources collected
- Unit movements
- Combat results
- AI decisions

### Win Condition:
Capture the enemy base (red cell in top-right corner) with one of your units!

## ❌ Troubleshooting

### Problem: "I don't see the Setup Game Scene button"
**Solution:** Make sure you added the SceneSetup component correctly:
1. Select your SceneSetup GameObject
2. Check Inspector - you should see "Scene Setup (Script)"
3. If you see "Script: None", delete the component and add it again

### Problem: "Nothing happens when I click Setup Game Scene"
**Solution:**
1. Look at the Console window (Window → General → Console)
2. Check for error messages
3. Make sure all scripts compiled (no red errors in Console)

### Problem: "I see the grid but no units"
**Solution:**
1. Check Console for "Spawned Infantry" messages
2. If you see errors about UnitStats, the hardcoded stats aren't loading
3. Try restarting Unity

### Problem: "The AI doesn't play on its turn"
**Solution:**
1. Make sure StrategicManager was created (check Hierarchy)
2. Check Console for "Strategic AI executing turn" messages
3. The AI takes 0.5-1 second delay - be patient!

### Problem: "I can't click on units"
**Solution:**
1. Make sure you're in Play mode (▶ button should be blue)
2. Make sure you're clicking in the Game tab, not Scene tab
3. The units need colliders - they should have them automatically

## 📊 Console Messages You'll See

Normal gameplay:
```
Game initialized! Player 0's turn.
Spawned Infantry for Player 0 at (0, 1)
Resource node placed at (4, 5)
Turn 0 - Player 0's turn. Resources: 100

[After you move a unit]
Turn 0 - Player 1's turn. Resources: 110

[AI's turn - automatic]
Strategic AI executing turn for Player 1
AI Assessment - Own units: 2, Enemy units: 2, Resources: 110
Infantry received order: AttackBase
Turn 1 - Player 0's turn. Resources: 110
```

## 🎓 Understanding What's Happening

The SceneSetup script automatically created:
1. **Main Camera** - So you can see the game
2. **HexGrid** - Generates the 10x10 hexagonal map
3. **GameManager** - Manages turns, units, resources
4. **InputManager** - Handles your mouse clicks
5. **StrategicManager** - The AI brain (controls Player 1)

All these are connected and ready to work!

## 🆘 Still Having Problems?

1. Check the Console for red error messages
2. Make sure you're using Unity 6000.0.40f1 (or compatible version)
3. Try: **Edit → Clear All PlayerPrefs** then restart Unity
4. Delete the SceneSetup GameObject and follow steps 2-4 again

## ✅ Success Checklist

After following all steps, you should have:
- [ ] SceneSetup GameObject in Hierarchy
- [ ] SceneSetup component in Inspector
- [ ] Clicked "Setup Game Scene" button
- [ ] See these in Hierarchy: Camera, HexGrid, GameManager, InputManager, StrategicManager
- [ ] Pressed Play
- [ ] See hexagonal grid
- [ ] See 4 units (2 blue, 2 red)
- [ ] Can click and move units
- [ ] Console shows game messages
- [ ] AI plays automatically after pressing SPACE

---

**If everything works:** Congratulations! 🎉 You can now play against the hierarchical AI!

**Next steps:** Try to capture the enemy base. The AI will defend and attack. Good luck!

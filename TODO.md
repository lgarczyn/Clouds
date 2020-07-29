### TODO

- Solve performance issues
    - shadow map is unsatifactory
        - poor perf
          - programmable pipeline
        - require recalculations
        - low precision
          - especially bad for distant shadows
          - higher precision first index?
          - only first index
            - which also allows easier interpolation
            - are the other three indices even useful?
        - except for light rays, kinda ugly
    - solutions
        - per frame ultra-targeted maps
            - fucking hard
        - slowly updated general maps
            - also hard
            - bad for any kind of large cloud mouvement
        - wait for RTX cards
    - porting distance-based density calculations?
        - using iteration count instead of distance?
    - using density derivative to avoid any bad gradient issue?
    - lighten cloud density maps for crying out loud
      - maybe use custom interpolation for perlin effect?
    
- Ship model
    - install and learn blender
    - checkout dad paperplane models

### Ideas

- Gameplay
    - Expand, extract, exterminate 
      - vs contemplate, communicate, care
    - core gameplay loop
      - find some kind of ressource
      - fight if necessary
      - upgrade ship
    - secondary loop
      - explore
      - upgrade
      - move on
    - game loop
      - crash on planet
      - move to red eye
      - make final choice
        - leave without answers
        - kill big final ship for H3 resupply and/or rare resources
        - work with final ship to call for help ?
          - (accept waiting a decade in the planet ?)

- Monsters
    - whales, sharp, jellyfish, plancton
    - fish schools using coding adventure tutorial
    - use density function for FUN! with cloud interaction
        - stay out of deep clouds
        - stay out of the open
        - recover resources from cloud borders

- Crew
    - AI
      - caring
      - voiced by gumshoe?
    - Military presence
      - Push for aggressive router
    - First science officer
      - Favors survival, wants the alien tech
    - Archeologist
      - Favors care and waiting for rescue (though unlikely)

- Sciance
    - FTL error vs AI error
    - H3 is both a fusion fuel and vital for animals trying to float without hydrogen
    - diamond dust in clouds would be extremely abrasive to ship
      - kind of a bluff, there might diamond rain in lower layers, but that hardly translates
    - 50km height of cloud layer
    - big wasteland underneat where air pressure gets dangerous
    - big wasteland above with too little lift to take off

- Choices
    - Danger to crew as a motivator for harm?
    - Make pacifist router harder? More interesting? Harder to stumble upon?

- Mirror on society?
    - What are future humans like?
    - Gender roles on the ship?
    - Transhumanism?
    - Value of cultural remains of the old Earth?
    - Value of humanity?
    - Value of the native life?
    - Is the only winning move not to play?
      - Spec Ops was kinda bad about this one

- Tweak graphics
  - Shadows
    - Use player position in lightmarch
  - Better UI circle.cross
  - Damage particles are not great, texture?

- Clarify gameplay loop
  - Resource bars should be instinctive
  - Controls should be displayable ( pause screen ? )

- Add gratification
  - Win time -> instant replay value

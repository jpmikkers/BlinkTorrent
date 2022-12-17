
class BlinkyAnimator {
    constructor(blinkyGroupElt) {
        this.leftEyeOpen = blinkyGroupElt.querySelector(".blinkyLeftEyeOpen");
        this.rightEyeOpen = blinkyGroupElt.querySelector(".blinkyRightEyeOpen");

        this.leftEyeClosed = blinkyGroupElt.querySelector(".blinkyLeftEyeClosed");
        this.rightEyeClosed = blinkyGroupElt.querySelector(".blinkyRightEyeClosed");

        this.leftPupil = this.leftEyeOpen.querySelector(".blinkyPupil");
        this.rightPupil = this.rightEyeOpen.querySelector(".blinkyPupil");

        this.eyesAreOpen = false;

        this.lpcx = Number(this.leftPupil.getAttributeNS(null, 'cx'));
        this.lpcy = Number(this.leftPupil.getAttributeNS(null, 'cy'));
        this.rpcx = Number(this.rightPupil.getAttributeNS(null, 'cx'));
        this.rpcy = Number(this.rightPupil.getAttributeNS(null, 'cy'));
        this.timer = -1;
        this.#startAnimation();
    }

    #startAnimation() {
        this.#updateEyes();
        this.#animateBlinky();
    }

    stopAnimation() {
        if (this.timer >= 0) {
            clearTimeout(this.timer);
        }
    }

    #getRandom(min, max) {
        return min + (Math.random() * (max - min));
    }

    #animateBlinky() {
        let nextTimeout = 200;

        if (this.#getRandom(1, 100) < 50) {
            this.#movePupils();
        }

        if (this.eyesAreOpen) {
            if (this.#getRandom(1, 100) < 20) {
                this.eyesAreOpen = false;
                this.#updateEyes();
                nextTimeout = 200;
            } else {
                nextTimeout = this.#getRandom(500, 1000);
            }
        } else {
            this.eyesAreOpen = true;
            this.#updateEyes();
            nextTimeout = this.#getRandom(500, 1000);
        }

        this.timer = setTimeout(() => { this.#animateBlinky() }, nextTimeout);
    }

    #updateEyes() {
        if (this.eyesAreOpen) {
            this.leftEyeOpen.setAttributeNS(null, 'visibility', 'visible');
            this.leftEyeClosed.setAttributeNS(null, 'visibility', 'hidden');
        }
        else {
            this.leftEyeOpen.setAttributeNS(null, 'visibility', 'hidden');
            this.leftEyeClosed.setAttributeNS(null, 'visibility', 'visible');
        }

        if (this.eyesAreOpen) {
            this.rightEyeOpen.setAttributeNS(null, 'visibility', 'visible');
            this.rightEyeClosed.setAttributeNS(null, 'visibility', 'hidden');
        }
        else {
            this.rightEyeOpen.setAttributeNS(null, 'visibility', 'hidden');
            this.rightEyeClosed.setAttributeNS(null, 'visibility', 'visible');
        }
    }

    #movePupils() {
        let dx = this.#getRandom(-4, 4);
        let dy = this.#getRandom(-4, 4);

        this.leftPupil.setAttributeNS(null, 'cx', this.lpcx + dx);
        this.leftPupil.setAttributeNS(null, 'cy', this.lpcy + dy);
        this.rightPupil.setAttributeNS(null, 'cx', this.rpcx + dx);
        this.rightPupil.setAttributeNS(null, 'cy', this.rpcy + dy);
    }
}

export function createAnimator(blinkyGroupElt) {
    return new BlinkyAnimator(blinkyGroupElt);
}

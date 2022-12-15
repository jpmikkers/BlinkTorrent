
class BlinkyAnimator {
    constructor(eyesOpenElt, eyesClosedElt, leftPupilElt, rightPupilElt) {
        this.eyesOpen = eyesOpenElt
        this.eyesClosed = eyesClosedElt
        this.leftPupil = leftPupilElt
        this.rightPupil = rightPupilElt
        this.eyesAreOpen = false
        this.lpcx = Number(this.leftPupil.getAttributeNS(null, 'cx'))
        this.lpcy = Number(this.leftPupil.getAttributeNS(null, 'cy'))
        this.rpcx = Number(this.rightPupil.getAttributeNS(null, 'cx'))
        this.rpcy = Number(this.rightPupil.getAttributeNS(null, 'cy'))
        this.timer = -1
        this.#startAnimation();
    }

    #startAnimation() {
        this.#closeEyes();
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
            this.#movePupils()
        }

        if (this.eyesAreOpen) {
            if (this.#getRandom(1, 100) < 20) {
                this.eyesAreOpen = false;
                this.#closeEyes()
                nextTimeout = 200;
            } else {
                nextTimeout = this.#getRandom(500, 1000);
            }
        } else {
            this.eyesAreOpen = true
            this.#openEyes()
            nextTimeout = this.#getRandom(500, 1000);
        }

        this.timer = setTimeout(() => { this.#animateBlinky() }, nextTimeout);
    }

    #closeEyes() {
        this.eyesOpen.setAttributeNS(null, 'visibility', 'hidden')
        this.eyesClosed.setAttributeNS(null, 'visibility', 'visible')
    }

    #openEyes() {
        this.eyesOpen.setAttributeNS(null, 'visibility', 'visible')
        this.eyesClosed.setAttributeNS(null, 'visibility', 'hidden')
    }

    #movePupils() {
        let dx = this.#getRandom(-4, 4)
        let dy = this.#getRandom(-4, 4)
        
        this.leftPupil.setAttributeNS(null, 'cx', this.lpcx + dx)
        this.leftPupil.setAttributeNS(null, 'cy', this.lpcy + dy)
        this.rightPupil.setAttributeNS(null, 'cx', this.rpcx + dx)
        this.rightPupil.setAttributeNS(null, 'cy', this.rpcy + dy)
    }
}

export function createAnimator(eyesOpenElt, eyesClosedElt, leftPupilElt, rightPupilElt) {
    return new BlinkyAnimator(eyesOpenElt, eyesClosedElt, leftPupilElt, rightPupilElt)
}

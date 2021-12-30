var widthgame = 800;
var heightgame = 600;
if (window.innerWidth < widthgame) {
    widthgame = window.innerWidth;
}
if (window.innerHeight < heightgame) {
    heightgame = window.innerHeight;
}
var config = {
    type: Phaser.WEBGL,
    width: widthgame,
    height: heightgame,
    backgroundColor: '#2d2d2d',
    parent: 'game-content',
    scene: {
        preload: preload,
        create: create
    }
};
var game = new Phaser.Game(config);

function preload() {
    this.load.atlas('tunafish', '../../../assets/fish.png', '../../../assets/fish.json');
    this.load.atlas('cartoonfish', '../../../assets/Cartoon.png', '../../../assets/Cartoon.json');
    this.load.atlas('emeraldofish', '../../../assets/emeraldo.png', '../../../assets/emeraldo.json');
    this.load.atlas('funnyfish', '../../../assets/funny.png', '../../../assets/funny.json');
    this.load.atlas('mosquitofish', '../../../assets/mosquito.png', '../../../assets/mosquito.json');
    this.load.atlas('scaryfish', '../../../assets/scary.png', '../../../assets/scary.json');
    this.load.atlas('goldfish', '../../../assets/gold.png', '../../../assets/gold.json');
    this.load.atlas('shadowfish', '../../../assets/shadow.png', '../../../assets/shadow.json');
    this.load.atlas('toxinfish', '../../../assets/toxin.png', '../../../assets/toxin.json');
    this.load.atlas('destroyfish', '../../../assets/destroy.png', '../../../assets/destroy.json');
    this.load.image('bg', '../../../assets/ui/undersea-bg.png');
}

function create() {
    this.add.image(400, 300, 'bg');
    const tuna = this.add.sprite(Phaser.Math.Between(1, 800), Phaser.Math.Between(1, 800), 'tunafish').setScale(0.35).setInteractive();
    const cartoon = this.add.sprite(Phaser.Math.Between(1, 800), Phaser.Math.Between(1, 800), 'cartoonfish').setScale(0.55).setInteractive();
    const emeraldo = this.add.sprite(Phaser.Math.Between(1, 800), Phaser.Math.Between(1, 800), 'emeraldofish').setScale(0.5).setInteractive();
    const funny = this.add.sprite(Phaser.Math.Between(1, 800), Phaser.Math.Between(1, 800), 'funnyfish').setScale(0.5).setInteractive();
    const mosquito = this.add.sprite(Phaser.Math.Between(1, 800), Phaser.Math.Between(1, 800), 'mosquitofish').setScale(0.5).setInteractive();
    const scary = this.add.sprite(Phaser.Math.Between(1, 800), Phaser.Math.Between(1, 800), 'scaryfish').setFlipX(true).setScale(0.3).setInteractive();
    const gold = this.add.sprite(Phaser.Math.Between(1, 800), Phaser.Math.Between(1, 800), 'goldfish').setScale(0.3).setInteractive();
    const shadow = this.add.sprite(Phaser.Math.Between(1, 800), Phaser.Math.Between(1, 800), 'shadowfish').setScale(0.5).setInteractive();
    const toxin = this.add.sprite(Phaser.Math.Between(1, 800), Phaser.Math.Between(1, 800), 'toxinfish').setScale(0.5).setInteractive();
    const destroy = this.add.sprite(Phaser.Math.Between(1, 800), Phaser.Math.Between(1, 800), 'destroyfish').setScale(0.5).setInteractive();

    destroy.anims.create({
        key: 'swim',
        frames: this.anims.generateFrameNames('destroyfish'),
        frameRate: 6,
        repeat: -1
    });
    toxin.anims.create({
        key: 'swim',
        frames: this.anims.generateFrameNames('toxinfish'),
        frameRate: 6,
        repeat: -1
    });
    shadow.anims.create({
        key: 'swim',
        frames: this.anims.generateFrameNames('shadowfish'),
        frameRate: 6,
        repeat: -1
    });
    gold.anims.create({
        key: 'swim',
        frames: this.anims.generateFrameNames('goldfish'),
        frameRate: 6,
        repeat: -1
    });
    scary.anims.create({
        key: 'swim',
        frames: this.anims.generateFrameNames('scaryfish'),
        frameRate: 6,
        repeat: -1
    });
    mosquito.anims.create({
        key: 'swim',
        frames: this.anims.generateFrameNames('mosquitofish'),
        frameRate: 8,
        repeat: -1
    });
    funny.anims.create({
        key: 'swim',
        frames: this.anims.generateFrameNames('funnyfish'),
        frameRate: 8,
        repeat: -1
    });
    tuna.anims.create({
        key: 'swim',
        frames: this.anims.generateFrameNames('tunafish'),
        frameRate: 8,
        repeat: -1
    });
    emeraldo.anims.create({
        key: 'swim',
        frames: this.anims.generateFrameNames('emeraldofish'),
        frameRate: 8,
        repeat: -1
    });
    cartoon.anims.create({
        key: 'swim',
        frames: this.anims.generateFrameNames('cartoonfish'),
        frameRate: 8,
        repeat: -1
    });
    tuna.play('swim');
    cartoon.play('swim');
    emeraldo.play('swim');
    funny.play('swim');
    mosquito.play('swim');
    scary.play('swim');
    gold.play('swim');
    shadow.play('swim');
    toxin.play('swim');
    destroy.play('swim');

    this.tweens.add({
        targets: destroy,
        props: {
            x: { value: 10, duration: 4000, flipX: true },
            y: { value: 10, duration: 8000, },
        },
        ease: 'Sine.easeInOut',
        yoyo: true,
        repeat: -1
    });
    this.tweens.add({
        targets: tuna,
        props: {
            x: { value: 10, duration: 4000, flipX: true },
            y: { value: 10, duration: 8000, },
        },
        ease: 'Sine.easeInOut',
        yoyo: true,
        repeat: -1
    });
    this.tweens.add({
        targets: cartoon,
        props: {
            x: { value: 13, duration: 2000, flipX: true },
            y: { value: 15, duration: 3000, },
        },
        ease: 'Sine.easeInOut',
        yoyo: true,
        repeat: -1
    });
    this.tweens.add({
        targets: emeraldo,
        props: {
            x: { value: 10, duration: 3200, flipX: true },
            y: { value: 10, duration: 5600, },
        },
        ease: 'Sine.easeInOut',
        yoyo: true,
        repeat: -1
    });
    this.tweens.add({
        targets: funny,
        props: {
            x: { value: 20, duration: 4200, flipX: true },
            y: { value: 2, duration: 2600, },
        },
        ease: 'Sine.easeInOut',
        yoyo: true,
        repeat: -1
    });
    this.tweens.add({
        targets: mosquito,
        props: {
            x: { value: 300, duration: 3000, flipX: true },
            y: { value: 2, duration: 6000, },
        },
        ease: 'Sine.easeInOut',
        yoyo: true,
        repeat: -1
    });
    this.tweens.add({
        targets: scary,
        props: {
            x: { value: 300, duration: 3000, flipX: true },
            y: { value: 2, duration: 6000, },
        },
        ease: 'Sine.easeInOut',
        yoyo: true,
        repeat: -1
    });
    this.tweens.add({
        targets: gold,
        props: {
            x: { value: 300, duration: 3000, flipX: true },
            y: { value: 2, duration: 6000, },
        },
        ease: 'Sine.easeInOut',
        yoyo: true,
        repeat: -1
    });
    this.tweens.add({
        targets: shadow,
        props: {
            x: { value: 300, duration: 3000, flipX: true },
            y: { value: 2, duration: 6000, },
        },
        ease: 'Sine.easeInOut',
        yoyo: true,
        repeat: -1
    });
    this.tweens.add({
        targets: toxin,
        props: {
            x: { value: 300, duration: 3000, flipX: true },
            y: { value: 2, duration: 6000, },
        },
        ease: 'Sine.easeInOut',
        yoyo: true,
        repeat: -1
    });
    var tweens = this.tweens;
    tweens.timeScale = 0.3;

    tuna.on('pointerdown', function (event) {
        alert("my name is tuna")
    });
}

